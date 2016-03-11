// -----------------------------------------------------------------------
// <copyright file="SubscriptionWithOfferViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using PartnerCenter.Models.Subscriptions;

    /// <summary>
    /// The subscription alongside its associated offer.
    /// </summary>
    public class SubscriptionWithOfferViewModel
    {
        /// <summary>
        /// Gets or sets the subscription.
        /// </summary>
        public Subscription Subscription { get; set; }

        /// <summary>
        /// Gets or sets the offer Id.
        /// </summary>
        public string OfferId { get; set; }
    }    
}