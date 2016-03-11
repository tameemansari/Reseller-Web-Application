// -----------------------------------------------------------------------
// <copyright file="SubscriptionViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    /// <summary>
    /// The subscription view model.
    /// </summary>
    public class SubscriptionViewModel
    {
        /// <summary>
        /// Gets or sets the subscription's friendly name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the subscription's quantity.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Gets or sets the subscription's price.
        /// </summary>
        public string Price { get; set; }
    }    
}