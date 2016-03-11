// -----------------------------------------------------------------------
// <copyright file="TemplateController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Configuration;
    using Configuration.Manager;

    /// <summary>
    /// Serves HTML templates to the browser.
    /// </summary>
    public class TemplateController : Controller
    {
        /// <summary>
        /// Serves the HTML template for the homepage presenter.
        /// </summary>
        /// <returns>The HTML template for the homepage presenter.</returns>
        [HttpGet]
        public ActionResult HomePage()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the offers presenter.
        /// </summary>
        /// <returns>The HTML template for the offers presenter.</returns>
        [HttpGet]
        public ActionResult Offers()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the customer registration presenter.
        /// </summary>
        /// <returns>The HTML template for the customer registration presenter.</returns>
        [HttpGet]
        public ActionResult CustomerRegistration()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the registration confirmation presenter.
        /// </summary>
        /// <returns>The HTML template for the registration confirmation presenter.</returns>
        [HttpGet]
        public ActionResult RegistrationConfirmation()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the customer account presenter.
        /// </summary>
        /// <returns>The HTML template for the homepage presenter.</returns>
        [HttpGet]
        public ActionResult CustomerAccount()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the add subscriptions presenter.
        /// </summary>
        /// <returns>The HTML template for the add subscriptions presenter.</returns>
        [HttpGet]
        public ActionResult AddSubscriptions()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the update subscriptions presenter.
        /// </summary>
        /// <returns>The HTML template for the update subscriptions presenter.</returns>
        [HttpGet]
        public ActionResult UpdateSubscriptions()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the update contact information presenter.
        /// </summary>
        /// <returns>The HTML template for the update contact information presenter.</returns>
        [HttpGet]
        public ActionResult UpdateContactInformation()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the update company information presenter.
        /// </summary>
        /// <returns>The HTML template for the update company information presenter.</returns>
        [HttpGet]
        public ActionResult UpdateCompanyInformation()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML templates for the framework controls and services.
        /// </summary>
        /// <returns>The HTML template for the framework controls and services.</returns>
        [HttpGet]
        public async Task<ActionResult> FrameworkFragments()
        {
            WebPortalConfigurationManager builder = ApplicationConfiguration.WebPortalConfigurationManager;
            ViewBag.Templates = (await builder.AggregateNonStartupAssets()).Templates;

            return this.PartialView();
        }
    }
}