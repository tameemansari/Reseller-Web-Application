// -----------------------------------------------------------------------
// <copyright file="CustomerRegistrationViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The customer registration view model.
    /// </summary>
    public class CustomerRegistrationViewModel
    {
        /// <summary>
        /// Gets or sets the orders the customer placed.
        /// </summary>
        public IEnumerable<OrderViewModel> Orders { get; set; }

        /// <summary>
        /// Gets or sets the customer information.
        /// </summary>
        public CustomerViewModel Customer { get; set; }
    }
}