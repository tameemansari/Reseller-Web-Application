// -----------------------------------------------------------------------
// <copyright file="BaseController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Controllers
{
    using System.Web;
    using System.Web.Http;
    using AspNet.Identity.Owin;
    using BusinessLogic;

    /// <summary>
    /// The base web API controller. All web API controllers should inherit from this class.
    /// </summary>
    public class BaseController : ApiController
    {
        /// <summary>
        /// The application sign in manager.
        /// </summary>
        private ApplicationSignInManager signInManager;

        /// <summary>
        /// The application user manager.
        /// </summary>
        private ApplicationUserManager userManager;

        /// <summary>
        /// Gets the application sign in manager.
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return this.signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }

            private set
            {
                this.signInManager = value;
            }
        }

        /// <summary>
        /// Gets the application user manager.
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return this.userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }

            private set
            {
                this.userManager = value;
            }
        }

        /// <summary>
        /// Disposes of the controller.
        /// </summary>
        /// <param name="disposing">A flag indicating a disposal is in progress or not.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.userManager != null)
                {
                    this.userManager.Dispose();
                    this.userManager = null;
                }

                if (this.signInManager != null)
                {
                    this.signInManager.Dispose();
                    this.signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
