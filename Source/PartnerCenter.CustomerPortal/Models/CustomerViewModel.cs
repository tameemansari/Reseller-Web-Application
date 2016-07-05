// -----------------------------------------------------------------------
// <copyright file="CustomerViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The customer view model.
    /// </summary>
    public class CustomerViewModel
    {
        /// <summary>
        /// Gets or sets the microsoft Id.
        /// </summary>
        public string MicrosoftId { get; set; }

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
        /// Gets or sets the customer's language. 
        /// </summary>
        public string Language { get; set; }        

        /// <summary>
        /// Gets or sets the customer's email.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the customer's password.
        /// </summary>
        public string Password { get; set; }

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
        [RegularExpression(@"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$")]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the customer's domain prefix.
        /// </summary>
        [Required]
        [RegularExpression("^[a-zA-Z0-9]*$")]
        public string DomainPrefix { get; set; }

        /// <summary>
        /// Gets or sets the admin user account.
        /// </summary>
        public string AdminUserAccount { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }
    }
}