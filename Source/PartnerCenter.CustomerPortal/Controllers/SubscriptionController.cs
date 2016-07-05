// -----------------------------------------------------------------------
// <copyright file="SubscriptionController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using BusinessLogic;
    using BusinessLogic.Commerce;
    using BusinessLogic.Commerce.PaymentGateways;
    using BusinessLogic.Exceptions;
    using Filters;
    using Filters.WebApi;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Manages customer subscriptions.
    /// </summary>
    [RoutePrefix("api/Subscription")]
    public class SubscriptionController : BaseController
    {        
        /// <summary>
        /// Retrieves a summary of all subscriptions and their respective order histories. 
        /// </summary>
        /// <returns>The Subscription summary used by the client used for rendering purposes.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Customer)]
        [Route("summary")]
        public async Task<SubscriptionsSummary> SubscriptionSummary()
        {
            return await this.GetSubscriptionSummary(this.Principal.PartnerCenterCustomerId);
        }

        /// <summary>
        /// Adds new subscriptions to an existing customer.
        /// </summary>
        /// <param name="orderUpdateInformation">Order Model which contains Subscription details and Credit Card to charge.</param>
        /// <returns>An HTTP response indicating the result of the addition.</returns>
        [@Authorize(UserRole = UserRole.Customer)]
        [HttpPost]
        [Route("")]
        public async Task AddSubscriptions([FromBody] OrderViewModel orderUpdateInformation)
        {
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);                
            }
            
            await this.AddCustomerSubscription(this.Principal.PartnerCenterCustomerId, orderUpdateInformation);
        }

        /// <summary>
        /// Updates a customer's subscriptions.
        /// </summary>
        /// <param name="orderUpdateInformation">A list of subscriptions to update.</param>
        /// <returns>The updated subscriptions.</returns>
        [@Authorize(UserRole = UserRole.Customer)]
        [HttpPost]
        [Route("AddSeats")]
        public async Task<TransactionResult> UpdateSubscription([FromBody] OrderViewModel orderUpdateInformation)
        {
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);                
            }

            string clientCustomerId = this.Principal.PartnerCenterCustomerId;            

            orderUpdateInformation.Subscriptions.AssertNotNull(nameof(orderUpdateInformation.Subscriptions));
            List<OrderSubscriptionItemViewModel> orderSubscriptions = orderUpdateInformation.Subscriptions.ToList();
            if (!(orderSubscriptions.Count == 1))
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", Resources.MoreThanOneSubscriptionUpdateErrorMessage);
            }

            string subscriptionId = orderSubscriptions.First().SubscriptionId;
            int additionalSeats = orderSubscriptions.First().Quantity;

            subscriptionId.AssertNotEmpty(nameof(subscriptionId));      // required for commerce operation. 
            additionalSeats.AssertPositive(nameof(additionalSeats));    // required & should be greater than 0. 

            string operationDescription = string.Format("Customer Id:[{0}] - Added to subscription:{1}.", clientCustomerId, subscriptionId);
            PayPalGateway paymentGateway = new PayPalGateway(ApplicationDomain.Instance, orderUpdateInformation.CreditCard, operationDescription);
            CommerceOperations commerceOperation = new CommerceOperations(ApplicationDomain.Instance, clientCustomerId, paymentGateway);

            return await commerceOperation.PurchaseAdditionalSeatsAsync(subscriptionId, additionalSeats);
        }

        /// <summary>
        /// Renew a customer's subscriptions.
        /// </summary>
        /// <param name="renewalOrderInformation">A list of subscriptions to update.</param>
        /// <returns>The updated subscriptions.</returns>
        [@Authorize(UserRole = UserRole.Customer)]
        [HttpPost]
        [Route("Renew")]
        public async Task<TransactionResult> RenewSubscription([FromBody] OrderViewModel renewalOrderInformation)
        {
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);                
            }

            string clientCustomerId = this.Principal.PartnerCenterCustomerId;

            renewalOrderInformation.Subscriptions.AssertNotNull(nameof(renewalOrderInformation.Subscriptions));
            List<OrderSubscriptionItemViewModel> orderSubscriptions = renewalOrderInformation.Subscriptions.ToList();
            if (!(orderSubscriptions.Count == 1))
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", Resources.MoreThanOneSubscriptionUpdateErrorMessage);
            }

            string subscriptionId = orderSubscriptions.First().SubscriptionId;
            subscriptionId.AssertNotEmpty(nameof(subscriptionId)); // is Required for the commerce operation. 

            string operationDescription = string.Format("Customer Id:[{0}] - Renewed subscription:{1}.", clientCustomerId, subscriptionId);
            PayPalGateway paymentGateway = new PayPalGateway(ApplicationDomain.Instance, renewalOrderInformation.CreditCard, operationDescription);
            CommerceOperations commerceOperation = new CommerceOperations(ApplicationDomain.Instance, clientCustomerId, paymentGateway);

            return await commerceOperation.RenewSubscriptionAsync(subscriptionId);
        }

        /// <summary>
        /// Adds new subscriptions to a newly registered customer.
        /// </summary>
        /// <param name="orderRegistrationInformation">The customer's subscriptions, credit card and Customer id.</param>
        /// <returns>A registration confirmation.</returns>        
        [HttpPost]
        [Route("RegistrationOrder")]
        [@Authorize(UserRole = UserRole.None)]
        public async Task<SubscriptionsSummary> RegisterAddSubscriptions([FromBody] OrderViewModel orderRegistrationInformation)
        {
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);                
            }
            
            string clientCustomerId = orderRegistrationInformation.CustomerId;
            if (string.IsNullOrWhiteSpace(clientCustomerId))
            {                
                string errorMessage = Resources.InvalidCustomerIdErrorMessage;
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);                
            }

            await this.AddCustomerSubscription(clientCustomerId, orderRegistrationInformation);
            
            return await this.GetSubscriptionSummary(clientCustomerId);
        }

        /// <summary>
        /// Adds a portal subscription for the Customer. 
        /// </summary>
        /// <param name="clientCustomerId">The Customer Id.</param>
        /// <param name="orderSubscriptions">The Order information containing credit card and list of subscriptions to add.</param>
        /// <returns>Transaction Result from commerce operation.</returns>
        private async Task<TransactionResult> AddCustomerSubscription(string clientCustomerId, OrderViewModel orderSubscriptions)
        {            
            string operationDescription = string.Format("Customer Id:[{0}] - Added Subscription(s).", clientCustomerId);
            
            PayPalGateway paymentGateway = new PayPalGateway(ApplicationDomain.Instance, orderSubscriptions.CreditCard, operationDescription);
            CommerceOperations commerceOperation = new CommerceOperations(ApplicationDomain.Instance, clientCustomerId, paymentGateway);

            List<PurchaseLineItem> orderItems = new List<PurchaseLineItem>();
            foreach (var orderItem in orderSubscriptions.Subscriptions)
            {
                string offerId = orderItem.OfferId;
                int quantity = orderItem.Quantity;

                offerId.AssertNotEmpty(nameof(offerId));        // required for commerce operation. 
                quantity.AssertPositive(nameof(quantity));      // required & should be greater than 0. 

                orderItems.Add(new PurchaseLineItem(offerId, quantity));
            }
            
            return await commerceOperation.PurchaseAsync(orderItems);
        }

        /// <summary>
        /// Gets the summary of subscriptions for a portal customer. 
        /// </summary>
        /// <param name="customerId">The customer Id.</param>
        /// <returns>Subscription Summary.</returns>
        private async Task<SubscriptionsSummary> GetSubscriptionSummary(string customerId)
        {          
            var customerSubscriptionsTask = ApplicationDomain.Instance.CustomerSubscriptionsRepository.RetrieveAsync(customerId);
            var customerSubscriptionsHistoryTask = ApplicationDomain.Instance.CustomerPurchasesRepository.RetrieveAsync(customerId);
            var allPartnerOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveAsync();
            await Task.WhenAll(customerSubscriptionsTask, customerSubscriptionsHistoryTask, allPartnerOffersTask);

            var customerSubscriptionsHistory = customerSubscriptionsHistoryTask.Result;            

            // retrieve all the partner offers to match against them
            IEnumerable<PartnerOffer> allPartnerOffers = allPartnerOffersTask.Result;

            // start building the summary.                 
            decimal summaryTotal = 0;

            // format all responses to client using portal locale. 
            CultureInfo responseCulture = new CultureInfo(ApplicationDomain.Instance.PortalLocalization.Locale);
            List<SubscriptionViewModel> customerSubscriptionsView = new List<SubscriptionViewModel>();
            
            // iterate through and build the list of customer's subscriptions. 
            foreach (var subscription in customerSubscriptionsTask.Result)
            {
                decimal subscriptionTotal = 0;
                int licenseTotal = 0;
                List<SubscriptionHistory> historyItems = new List<SubscriptionHistory>();

                // collect the list of history items for this subcription.  
                var subscriptionHistoryList = customerSubscriptionsHistory
                    .Where(historyItem => historyItem.SubscriptionId == subscription.SubscriptionId)
                    .OrderBy(historyItem => historyItem.TransactionDate);

                // iterate through and build the SubsriptionHistory for this subscription. 
                foreach (var historyItem in subscriptionHistoryList)
                {
                    decimal orderTotal = Math.Round(historyItem.SeatPrice * historyItem.SeatsBought, 2);                    
                    historyItems.Add(new SubscriptionHistory()
                    {
                        OrderTotal = orderTotal.ToString("C", responseCulture),
                        PricePerSeat = historyItem.SeatPrice.ToString("C", responseCulture),                    // Currency format. 
                        SeatsBought = historyItem.SeatsBought.ToString("G", responseCulture),                   // General format.  
                        OrderDate = historyItem.TransactionDate.ToLocalTime().ToString("d", responseCulture),   // Short date format. 
                        OperationType = this.GetOperationType(historyItem.PurchaseType)                         // Localized Operation type string. 
                    });

                    // Increment the subscription total. 
                    licenseTotal += historyItem.SeatsBought;

                    // Increment the subscription total. 
                    subscriptionTotal += orderTotal;
                }

                var partnerOfferItem = allPartnerOffers.Where(offer => offer.Id == subscription.PartnerOfferId).FirstOrDefault();
                string subscriptionTitle = partnerOfferItem.Title;
                string portalOfferId = partnerOfferItem.Id;
                decimal portalOfferPrice = partnerOfferItem.Price;

                DateTime subscriptionExpiryDate = subscription.ExpiryDate.ToUniversalTime();
                int remainingDays = (subscriptionExpiryDate.Date - DateTime.UtcNow.Date).Days;
                bool isRenewable = remainingDays <= 30;
                bool isEditable = DateTime.UtcNow.Date <= subscriptionExpiryDate.Date;                
                
                if (partnerOfferItem.IsInactive)
                {
                    // in case the offer is inactive (marked for deletion) then dont allow renewals or editing on this subscription tied to this offer. 
                    isRenewable = false;
                    isEditable = false; 
                }

                // Compute the pro rated price per seat for this subcription & return for client side processing during updates. 
                decimal proratedPerSeatPrice = Math.Round(CommerceOperations.CalculateProratedSeatCharge(subscription.ExpiryDate, portalOfferPrice), 2);

                SubscriptionViewModel subscriptionItem = new SubscriptionViewModel()
                {
                    SubscriptionId = subscription.SubscriptionId,                    
                    FriendlyName = subscriptionTitle,
                    PortalOfferId = portalOfferId, 
                    PortalOfferPrice = portalOfferPrice.ToString("C", responseCulture),
                    IsRenewable = isRenewable,                                                              // IsRenewable is true if subscription is going to expire in 30 days.                         
                    IsEditable = isEditable,                                                                // IsEditable is true if today is lesser or equal to subscription expiry date.                                                
                    LicensesTotal = licenseTotal.ToString("G", responseCulture),                            // General format. 
                    SubscriptionTotal = subscriptionTotal.ToString("C", responseCulture),                   // Currency format.
                    SubscriptionExpiryDate = subscriptionExpiryDate.Date.ToString("d", responseCulture),    // Short date format. 
                    SubscriptionOrderHistory = historyItems,
                    SubscriptionProRatedPrice = proratedPerSeatPrice 
                };

                // add this subcription to the customer's subscription list.
                customerSubscriptionsView.Add(subscriptionItem);

                // Increment the summary total. 
                summaryTotal += subscriptionTotal;
            }

            // Sort List of subscriptions based on portal offer name. 
            return new SubscriptionsSummary()
            {
                Subscriptions = customerSubscriptionsView.OrderBy(subscriptionItem => subscriptionItem.FriendlyName),
                SummaryTotal = summaryTotal.ToString("C", responseCulture)      // Currency format.
            };
        }

        /// <summary>
        /// Retrieves the localized operation type string. 
        /// </summary>
        /// <param name="operationType">The Commerce operation type.</param>
        /// <returns>Localized Operation Type string.</returns>
        private string GetOperationType(CommerceOperationType operationType)
        {            
            switch (operationType)
            {
                case CommerceOperationType.AdditionalSeatsPurchase:
                    return Resources.CommerceOperationTypeAddSeats;
                case CommerceOperationType.NewPurchase:
                    return Resources.CommerceOperationTypeAddSubscription;
                case CommerceOperationType.Renewal:
                    return Resources.CommerceOperationTypeRenewSubscription;
                default:
                    return string.Empty;
            }
        }
    }
}
