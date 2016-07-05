// -----------------------------------------------------------------------
// <copyright file="OrderSubscriptionItemViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The order view model.
    /// </summary>
    public class OrderSubscriptionItemViewModel
    {
        /// <summary>
        /// Gets or sets the offer Id tied to the order.
        /// </summary>        
        public string OfferId { get; set; }

        /// <summary>
        /// Gets or sets the subscription Id (used primarily during update calls). 
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the offer being ordered.
        /// </summary>                
        public int Quantity { get; set; }
    }
}