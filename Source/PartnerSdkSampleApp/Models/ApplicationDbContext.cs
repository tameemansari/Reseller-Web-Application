// -----------------------------------------------------------------------
// <copyright file="ApplicationDbContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Models
{
    using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// Implements an application database context.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// Creates a new application database context.
        /// </summary>
        /// <returns>An application database context.</returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}