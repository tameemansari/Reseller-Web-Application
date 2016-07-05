// -----------------------------------------------------------------------
// <copyright file="ICommerceOperations.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// A contract for components implementing commerce operations.
    /// </summary>
    public interface ICommerceOperations
    {
        /// <summary>
        /// Gets the customer ID who owns the transaction.
        /// </summary>
        string CustomerId { get; }

        /// <summary>
        /// Gets the payment gateway used to process payments.
        /// </summary>
        IPaymentGateway PaymentGateway { get; }

        /// <summary>
        /// Purchases one or more partner offers.
        /// </summary>
        /// <param name="purchaseLineItems">A collection of purchase lines items to buy.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        Task<TransactionResult> PurchaseAsync(IEnumerable<PurchaseLineItem> purchaseLineItems);

        /// <summary>
        /// Purchases additional seats for an existing subscription the customer has already bought.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription for which to increase its quantity.</param>
        /// <param name="seatsToPurchase">The number of new seats to purchase on top of the existing ones.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        Task<TransactionResult> PurchaseAdditionalSeatsAsync(string subscriptionId, int seatsToPurchase);

        /// <summary>
        /// Renews an existing subscription for a customer.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription to renew.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        Task<TransactionResult> RenewSubscriptionAsync(string subscriptionId);
    }
}