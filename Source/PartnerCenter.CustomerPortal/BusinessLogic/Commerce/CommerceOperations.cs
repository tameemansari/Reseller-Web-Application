// -----------------------------------------------------------------------
// <copyright file="CommerceOperations.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Infrastructure;
    using Models;
    using PartnerCenter.Models.Orders;
    using Transactions;

    /// <summary>
    /// Implements the portal commerce transactions.
    /// </summary>
    public class CommerceOperations : DomainObject, ICommerceOperations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommerceOperations"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        /// <param name="customerId">The customer ID who owns the transaction.</param>
        /// <param name="paymentGateway">A payment gateway to use for processing payments resulting from the transaction.</param>
        public CommerceOperations(ApplicationDomain applicationDomain, string customerId, IPaymentGateway paymentGateway) : base(applicationDomain)
        {
            customerId.AssertNotEmpty(nameof(customerId));
            paymentGateway.AssertNotNull(nameof(paymentGateway));

            this.CustomerId = customerId;
            this.PaymentGateway = paymentGateway;
        }

        /// <summary>
        /// Gets the customer ID who owns the transaction.
        /// </summary>
        public string CustomerId { get; private set; }

        /// <summary>
        /// Gets the payment gateway used to process payments.
        /// </summary>
        public IPaymentGateway PaymentGateway { get; private set; }

        /// <summary>
        /// Calculates the amount to charge for buying an extra additional seat for the remainder of a subscription's lease.
        /// </summary>
        /// <param name="expiryDate">The subscription's expiry date.</param>
        /// <param name="yearlyRatePerSeat">The subscription's yearly price per seat.</param>
        /// <returns>The prorated amount to charge for the new extra seat.</returns>
        public static decimal CalculateProratedSeatCharge(DateTime expiryDate, decimal yearlyRatePerSeat)
        {
            DateTime rightNow = DateTime.UtcNow;
            expiryDate = expiryDate.ToUniversalTime();

            decimal dailyChargePerSeat = yearlyRatePerSeat / 365m;

            // TODO :: Review these conversions. All date based calculations need to be done using datime.date. 
            // round up the remaining days in case there was a fraction and ensure it does not exceed 365 days
            decimal remainingDaysTillExpiry = Math.Ceiling(Convert.ToDecimal((expiryDate - rightNow).TotalDays));
            remainingDaysTillExpiry = Math.Min(remainingDaysTillExpiry, 365);

            return remainingDaysTillExpiry * dailyChargePerSeat;
        }

        /// <summary>
        /// Purchases one or more partner offers.
        /// </summary>
        /// <param name="purchaseLineItems">A collection of purchase lines items to buy.</param>
        /// <returns>A transaction result which summarizes its outcomes.</returns>
        public async Task<TransactionResult> PurchaseAsync(IEnumerable<PurchaseLineItem> purchaseLineItems)
        {
            purchaseLineItems.AssertNotNull(nameof(purchaseLineItems));

            if (purchaseLineItems.Count() <= 0)
            {
                throw new ArgumentException("PurchaseLineItems should have at least one entry", nameof(purchaseLineItems));
            }

            var lineItemsWithOffers = await this.AssociateWithPartnerOffersAsync(purchaseLineItems);
            ICollection<IBusinessTransaction> subTransactions = new List<IBusinessTransaction>();

            decimal orderTotalPrice = Math.Round(this.CalculateOrderTotalPrice(lineItemsWithOffers), 2);

            // we have the order total, prepare payment authorization
            var paymentAuthorization = new AuthorizePayment(this.PaymentGateway, orderTotalPrice);
            subTransactions.Add(paymentAuthorization);

            // build the Partner Center order and pass it to the place order transaction
            Order partnerCenterPurchaseOrder = this.BuildPartnerCenterOrder(lineItemsWithOffers);

            var placeOrder = new PlaceOrder(
                this.ApplicationDomain.PartnerCenterClient.Customers.ById(this.CustomerId),
                partnerCenterPurchaseOrder);
            subTransactions.Add(placeOrder);

            // configure a transaction to save the new resulting subscriptions and purchases into persistence
            var persistSubscriptionsAndPurchases = new PersistNewlyPurchasedSubscriptions(
                this.CustomerId,
                this.ApplicationDomain.CustomerSubscriptionsRepository,
                this.ApplicationDomain.CustomerPurchasesRepository,
                () => new Tuple<Order, IEnumerable<PurchaseLineItemWithOffer>>(placeOrder.Result, lineItemsWithOffers));

            subTransactions.Add(persistSubscriptionsAndPurchases);

            // configure a capture payment transaction and let it read the auth code from the payment authorization output
            var capturePayment = new CapturePayment(this.PaymentGateway, () => paymentAuthorization.Result);
            subTransactions.Add(capturePayment);
            
            // build an aggregated transaction from the previous steps and execute it as a whole
            await CommerceOperations.RunAggregatedTransaction(subTransactions);
            
            return new TransactionResult(orderTotalPrice, persistSubscriptionsAndPurchases.Result, DateTime.UtcNow);
        }

        /// <summary>
        /// Purchases additional seats for an existing subscription the customer has already bought.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription for which to increase its quantity.</param>
        /// <param name="seatsToPurchase">The number of new seats to purchase on top of the existing ones.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        public async Task<TransactionResult> PurchaseAdditionalSeatsAsync(string subscriptionId, int seatsToPurchase)
        {
            // validate inputs
            subscriptionId.AssertNotEmpty("subscriptionId");
            seatsToPurchase.AssertPositive("seatsToPurchase");

            // we will add up the transactions here
            ICollection<IBusinessTransaction> subTransactions = new List<IBusinessTransaction>();

            // determine the prorated seat charge
            var subscriptionToAugment = await this.GetSubscriptionAsync(subscriptionId);
            var partnerOffer = await this.ApplicationDomain.OffersRepository.RetrieveAsync(subscriptionToAugment.PartnerOfferId);

            // if subscription expiry date.Date is less than today's UTC date then subcription has expired. 
            if (subscriptionToAugment.ExpiryDate.Date < DateTime.UtcNow.Date)
            {
                // this subscription has already expired, don't permit adding seats until the subscription is renewed
                throw new PartnerDomainException(ErrorCode.SubscriptionExpired);
            }

            decimal proratedSeatCharge = Math.Round(CommerceOperations.CalculateProratedSeatCharge(subscriptionToAugment.ExpiryDate, partnerOffer.Price), 2);
            decimal totalCharge = Math.Round(proratedSeatCharge * seatsToPurchase, 2);

            // configure a transaction to charge the payment gateway with the prorated rate
            var paymentAuthorization = new AuthorizePayment(this.PaymentGateway, totalCharge);
            subTransactions.Add(paymentAuthorization);

            // configure a purchase additional seats transaction with the requested seats to purchase
            subTransactions.Add(new PurchaseExtraSeats(
                this.ApplicationDomain.PartnerCenterClient.Customers.ById(this.CustomerId).Subscriptions.ById(subscriptionId),
                seatsToPurchase));

            DateTime rightNow = DateTime.UtcNow;

            // record the purchase in our purchase store
            subTransactions.Add(new RecordPurchase(
                this.ApplicationDomain.CustomerPurchasesRepository,
                new CustomerPurchaseEntity(CommerceOperationType.AdditionalSeatsPurchase, Guid.NewGuid().ToString(), this.CustomerId, subscriptionId, seatsToPurchase, proratedSeatCharge, rightNow)));

            // add a capture payment to the transaction pipeline
            subTransactions.Add(new CapturePayment(this.PaymentGateway, () => paymentAuthorization.Result));
            
            // build an aggregated transaction from the previous steps and execute it as a whole
            await CommerceOperations.RunAggregatedTransaction(subTransactions);

            var additionalSeatsPurchaseResult = new TransactionResultLineItem(
                subscriptionId,
                subscriptionToAugment.PartnerOfferId,
                seatsToPurchase,
                proratedSeatCharge,
                seatsToPurchase * proratedSeatCharge);

            return new TransactionResult(
                totalCharge,
                new TransactionResultLineItem[] { additionalSeatsPurchaseResult },
                rightNow);
        }

        /// <summary>
        /// Renews an existing subscription for a customer.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription to renew.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        public async Task<TransactionResult> RenewSubscriptionAsync(string subscriptionId)
        {
            // validate inputs
            subscriptionId.AssertNotEmpty("subscriptionId");

            // grab the customer subscription from our store
            var subscriptionToAugment = await this.GetSubscriptionAsync(subscriptionId);

            // retrieve the partner offer this subscription relates to, we need to know the current price
            var partnerOffer = await this.ApplicationDomain.OffersRepository.RetrieveAsync(subscriptionToAugment.PartnerOfferId);

            if (partnerOffer.IsInactive)
            {
                // renewing deleted offers is prohibited
                throw new PartnerDomainException(ErrorCode.PurchaseDeletedOfferNotAllowed).AddDetail("Id", partnerOffer.Id);
            }

            // retrieve the subscription from Partner Center
            var subscriptionOperations = this.ApplicationDomain.PartnerCenterClient.Customers.ById(this.CustomerId).Subscriptions.ById(subscriptionId);
            var partnerCenterSubscription = await subscriptionOperations.GetAsync();

            // we will add up the transactions here
            ICollection<IBusinessTransaction> subTransactions = new List<IBusinessTransaction>();
            decimal totalCharge = Math.Round(partnerCenterSubscription.Quantity * partnerOffer.Price, 2);

            // configure a transaction to charge the payment gateway with the prorated rate
            var paymentAuthorization = new AuthorizePayment(this.PaymentGateway, totalCharge);
            subTransactions.Add(paymentAuthorization);

            // add a renew subscription transaction to the pipeline
            subTransactions.Add(new RenewSubscription(
                subscriptionOperations,
                partnerCenterSubscription));

            DateTime rightNow = DateTime.UtcNow;

            // record the renewal in our purchase store
            subTransactions.Add(new RecordPurchase(
                this.ApplicationDomain.CustomerPurchasesRepository,
                new CustomerPurchaseEntity(CommerceOperationType.Renewal, Guid.NewGuid().ToString(), this.CustomerId, subscriptionId, partnerCenterSubscription.Quantity, partnerOffer.Price, rightNow)));

            // extend the expiry date by one year
            subTransactions.Add(new UpdatePersistedSubscription(
                this.ApplicationDomain.CustomerSubscriptionsRepository,
                new CustomerSubscriptionEntity(this.CustomerId, subscriptionId, partnerOffer.Id, subscriptionToAugment.ExpiryDate.AddYears(1))));           

            // add a capture payment to the transaction pipeline
            subTransactions.Add(new CapturePayment(this.PaymentGateway, () => paymentAuthorization.Result));

            // run the pipeline
            await CommerceOperations.RunAggregatedTransaction(subTransactions);

            var renewSubscriptionResult = new TransactionResultLineItem(
                subscriptionId,
                partnerOffer.Id,
                partnerCenterSubscription.Quantity,
                partnerOffer.Price,
                totalCharge);

            return new TransactionResult(
                totalCharge,
                new TransactionResultLineItem[] { renewSubscriptionResult },
                rightNow);
        }

        /// <summary>
        /// Runs a given list of transactions as a whole.
        /// </summary>
        /// <param name="subTransactions">A collection of transactions to run.</param>
        /// <returns>A task.</returns>
        private static async Task RunAggregatedTransaction(IEnumerable<IBusinessTransaction> subTransactions)
        {
            // build an aggregated transaction from the given transactions
            var aggregateTransaction = new SequentialAggregateTransaction(subTransactions);

            try
            {
                // execute it
                await aggregateTransaction.ExecuteAsync();
            }
            catch (Exception transactionFailure)
            {
                if (transactionFailure.IsFatal())
                {
                    throw;
                }

                // roll back the whole transaction
                await aggregateTransaction.RollbackAsync();

                // report the error
                throw;
            }
        }

        /// <summary>
        /// Retrieves a customer subscription from persistence.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <returns>The matching subscription.</returns>
        private async Task<CustomerSubscriptionEntity> GetSubscriptionAsync(string subscriptionId)
        {
            // grab the customer subscription from our store
            var customerSubscriptions = await this.ApplicationDomain.CustomerSubscriptionsRepository.RetrieveAsync(this.CustomerId);
            var subscriptionToAugment = customerSubscriptions.Where(subscription => subscription.SubscriptionId == subscriptionId).FirstOrDefault();

            if (subscriptionToAugment == null)
            {
                throw new PartnerDomainException(ErrorCode.SubscriptionNotFound);
            }

            return subscriptionToAugment;
        }

        /// <summary>
        /// Binds each purchase line item with the partner offer it is requesting.
        /// </summary>
        /// <param name="purchaseLineItems">A collection of purchase line items.</param>
        /// <returns>The requested association.</returns>
        private async Task<IEnumerable<PurchaseLineItemWithOffer>> AssociateWithPartnerOffersAsync(IEnumerable<PurchaseLineItem> purchaseLineItems)
        {
            // retrieve all the partner offers to match against them
            IEnumerable<PartnerOffer> allPartnerOffers = await this.ApplicationDomain.OffersRepository.RetrieveAsync();

            ICollection<PurchaseLineItemWithOffer> lineItemToOfferAssociations = new List<PurchaseLineItemWithOffer>();

            foreach (var lineItem in purchaseLineItems)
            {
                if (lineItem == null)
                {
                    throw new ArgumentException("a line item is null");
                }

                PartnerOffer offerToPurchase = allPartnerOffers.Where(offer => offer.Id == lineItem.PartnerOfferId).FirstOrDefault();

                if (offerToPurchase == null)
                {
                    // oops, this offer Id is unknown to us
                    throw new PartnerDomainException(ErrorCode.PartnerOfferNotFound).AddDetail("Id", lineItem.PartnerOfferId);
                }
                else if (offerToPurchase.IsInactive)
                {
                    // purchasing deleted offers is prohibited
                    throw new PartnerDomainException(ErrorCode.PurchaseDeletedOfferNotAllowed).AddDetail("Id", offerToPurchase.Id);
                }

                // associate the line item with the partner offer
                lineItemToOfferAssociations.Add(new PurchaseLineItemWithOffer(lineItem, offerToPurchase));
            }

            return lineItemToOfferAssociations;
        }

        /// <summary>
        /// Calculates the total price for a given purchase line items collection.
        /// </summary>
        /// <param name="purchaseLineItems">A list of purchase line items.</param>
        /// <returns>The total price.</returns>
        private decimal CalculateOrderTotalPrice(IEnumerable<PurchaseLineItemWithOffer> purchaseLineItems)
        {
            decimal orderTotalPrice = 0;

            foreach (var lineItem in purchaseLineItems)
            {
                orderTotalPrice += lineItem.PurchaseLineItem.Quantity * lineItem.PartnerOffer.Price;
            }

            return orderTotalPrice;
        }

        /// <summary>
        /// Builds a Microsoft Partner Center order from a list of purchase line items.
        /// </summary>
        /// <param name="purchaseLineItems">The purchase line items.</param>
        /// <returns>The Partner Center Order.</returns>
        private Order BuildPartnerCenterOrder(IEnumerable<PurchaseLineItemWithOffer> purchaseLineItems)
        {
            int lineItemNumber = 0;
            ICollection<OrderLineItem> partnerCenterOrderLineItems = new List<OrderLineItem>();

            // build the Partner Center order line items
            foreach (var lineItem in purchaseLineItems)
            {
                // add the line items to the partner center order and calculate the price to charge
                partnerCenterOrderLineItems.Add(new OrderLineItem()
                {
                    OfferId = lineItem.PartnerOffer.MicrosoftOfferId,
                    Quantity = lineItem.PurchaseLineItem.Quantity,
                    LineItemNumber = lineItemNumber++
                });
            }

            // bundle the order line items into a partner center order
            return new Order()
            {
                ReferenceCustomerId = this.CustomerId,
                LineItems = partnerCenterOrderLineItems
            };
        }
    }
}