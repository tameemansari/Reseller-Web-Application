// -----------------------------------------------------------------------
// <copyright file="RegistrationConfirmationViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The registration confirmation view model.
    /// </summary>
    public class RegistrationConfirmationViewModel
    {
        /// <summary>
        /// Gets or sets the microsoft Id.
        /// </summary>
        public string MicrosoftId { get; set; }

        /// <summary>
        /// Gets or sets the admin user account.
        /// </summary>
        public string AdminUserAccount { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the subscriptions bought.
        /// </summary>
        public IEnumerable<SubscriptionViewModel> Subscriptions { get; set; }

        /// <summary>
        /// Gets or sets the advisor Id.
        /// </summary>
        public string AdvisorId { get; set; }

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the first address line.
        /// </summary>
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the second address line.
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the website.
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the credit card number.
        /// </summary>
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the credit card expiry date.
        /// </summary>
        public string CreditCardExpiry { get; set; }

        /// <summary>
        /// Gets or sets the credit card type.
        /// </summary>
        public string CreditCardType { get; set; }
    }
}