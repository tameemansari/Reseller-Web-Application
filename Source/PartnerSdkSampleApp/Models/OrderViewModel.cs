// -----------------------------------------------------------------------
// <copyright file="OrderViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The order view model.
    /// </summary>
    public class OrderViewModel
    {
        /// <summary>
        /// Gets or sets the offer Id tied to the order.
        /// </summary>
        [Required]
        public string OfferId { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the order.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the offer being ordered.
        /// </summary>
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
    }
}