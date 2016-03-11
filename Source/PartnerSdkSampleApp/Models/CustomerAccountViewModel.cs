// -----------------------------------------------------------------------
// <copyright file="CustomerAccountViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using System.Collections.Generic;
    
    /// <summary>
    /// Holds customer account properties.
    /// </summary>
    public class CustomerAccountViewModel
    {
        /// <summary>
        /// Gets or sets the customer bought subscriptions.
        /// </summary>
        public IEnumerable<SubscriptionViewModel> Subscriptions { get; set; }

        /// <summary>
        /// Gets or sets the customer's advisor Id.
        /// </summary>
        public string AdvisorId { get; set; }

        /// <summary>
        /// Gets or sets the customer's company name.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the customer's first address line.
        /// </summary>
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the customer's second address line.
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the customer's city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the customer's state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the customer's zip code.
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the customer's country.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the customer's website.
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets the customer's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the customer's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the customer's language.
        /// </summary>
        public string Language { get; set; }
    }
}