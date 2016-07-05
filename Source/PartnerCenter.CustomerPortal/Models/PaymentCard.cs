// -----------------------------------------------------------------------
// <copyright file="PaymentCard.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    using System.ComponentModel.DataAnnotations;
    using Validators;

    /// <summary>
    /// The Credit Card payment instrument view model.
    /// </summary>
    public class PaymentCard
    {
        /// <summary>
        /// Gets or sets the customer's credit card type.
        /// </summary>
        [Required]
        public string CreditCardType { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card holder first name.
        /// </summary>
        [Required]
        public string CardHolderFirstName { get; set; }        

        /// <summary>
        /// Gets or sets the customer's credit card holder last name.
        /// </summary>
        [Required]
        public string CardHolderLastName { get; set; }        

        /// <summary>
        /// Gets or sets the customer's credit card number.
        /// </summary>
        [Required]
        [CreditCard]        
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card expiry month.
        /// </summary>
        [Required]
        [Range(1, 12)]
        public int CreditCardExpiryMonth { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card expiry year.
        /// </summary>
        [ExpiryDateInTenYears]
        public int CreditCardExpiryYear { get; set; }

        /// <summary>
        /// Gets or sets the customer's credit card CVN.
        /// Minimum 3 (4 in case of AMEX).
        /// </summary>
        [MinLength(3)]
        [MaxLength(4)]
        public string CreditCardCvn { get; set; }
    }
}