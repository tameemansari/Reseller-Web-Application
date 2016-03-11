// -----------------------------------------------------------------------
// <copyright file="Startup.Auth.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication
{
    using System;
    using AspNet.Identity;
    using AspNet.Identity.Owin;
    using BusinessLogic;
    using Models;
    using global::Owin;
    using Owin.Security.Cookies;

    /// <summary>
    /// Manages the application start up.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configured the application authentication.
        /// </summary>
        /// <param name="application">The application.</param>
        public void ConfigureAuth(IAppBuilder application)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            application.CreatePerOwinContext(ApplicationDbContext.Create);
            application.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            application.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            application.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),
                        OnApplyRedirect = ctx => { }
                }
            });
             
            application.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            application.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            application.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
        }
    }
}