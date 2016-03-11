// -----------------------------------------------------------------------
// <copyright file="CustomerViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The customer view model.
    /// </summary>
    public class CustomerViewModel
    {
        /// <summary>
        /// Gets or sets the customer's advisor Id.
        /// </summary>
        public string AdvisorId { get; set; }

        /// <summary>
        /// Gets or sets the customer's country.
        /// </summary>
        [Required]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the customer's company name.
        /// </summary>
        [Required]
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the customer's first address line.
        /// </summary>
        [Required]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the customer's second address line.
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the customer's city.
        /// </summary>
        [Required]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the customer's state.
        /// </summary>
        [Required]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the customer's zip code.
        /// </summary>
        [Required]
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the customer's website.
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets the customer's email.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the customer's password.
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the customer's password confirmation.
        /// </summary>
        [Required]
        [CompareAttribute("Password")]
        public string PasswordConfirmation { get; set; }

        /// <summary>
        /// Gets or sets the customer's first name.
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer's last name.
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the customer's phone number.
        /// </summary>
        [Required]
        [Phone]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the customer's phone extension.
        /// </summary>
        [Range(1, 5)]
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the customer's domain prefix.
        /// </summary>
        [Required]
        [RegularExpression("^[a-zA-Z0-9]*$")]
        public string DomainPrefix { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card type.
        /// </summary>
        [Required]
        public string CreditCardType { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card holder name.
        /// </summary>
        [Required]
        public string CreditCardHolderName { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card number.
        /// </summary>
        [Required]
        [MinLength(16)]
        [MaxLength(16)]
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card expiry month.
        /// </summary>
        [Required]
        [Range(1, 12)]
        public string CreditCardExpiryMonth { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card expiry year.
        /// </summary>
        [Range(2015, 2022)]
        public string CreditCardExpiryYear { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card CVN.
        /// </summary>
        [MinLength(3)]
        [MaxLength(3)]
        public string CreditCardCvn { get; set; }
    }
}