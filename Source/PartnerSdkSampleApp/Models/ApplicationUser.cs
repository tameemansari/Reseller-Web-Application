// -----------------------------------------------------------------------
// <copyright file="ApplicationUser.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// Holds attributes for the application user.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the user customer Id in Partner Center.
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Generates a new user identity.
        /// </summary>
        /// <param name="userManager">A user manager instance.</param>
        /// <returns>A new user identity.</returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> userManager)
        {
            return await userManager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}