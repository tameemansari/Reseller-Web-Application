// -----------------------------------------------------------------------
// <copyright file="ApplicationSignInManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.BusinessLogic
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using AspNet.Identity.Owin;
    using Models;
    using Owin;
    using Owin.Security;

    /// <summary>
    /// Configures the application sign-in manager which is used in this application.
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSignInManager"/> class.
        /// </summary>
        /// <param name="userManager">An application user manager.</param>
        /// <param name="authenticationManager">An authentication manager.</param>
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        /// <summary>
        /// Creates an application sign in manager instance.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="context">The context.</param>
        /// <returns>An application sign in manager.</returns>
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        /// <summary>
        /// Creates a user identity.
        /// </summary>
        /// <param name="user">The application user.</param>
        /// <returns>The user identity.</returns>
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }
    }
}
