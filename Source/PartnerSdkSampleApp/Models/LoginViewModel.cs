// -----------------------------------------------------------------------
// <copyright file="LoginViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The login view model.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user login should be persisted for sometime.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
