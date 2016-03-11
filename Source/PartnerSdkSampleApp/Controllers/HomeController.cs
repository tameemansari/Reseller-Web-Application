// -----------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Configuration;
    using Configuration.Bundling;
    using Configuration.Manager;
    using Configuration.WebPortal;
    using Newtonsoft.Json;

    /// <summary>
    /// Manages the application home page requests.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Serves the single page application to the browser.
        /// </summary>
        /// <returns>The SPA markup.</returns>
        public async Task<ActionResult> Index()
        {
            try
            {
                ViewBag.CurrentUser = "anonymous";

                WebPortalConfigurationManager builder = ApplicationConfiguration.WebPortalConfigurationManager;
                PluginsSegment clientVisiblePlugins = await builder.GeneratePlugins();
                IDictionary<string, dynamic> clientConfiguration = ApplicationConfiguration.ClientConfiguration;

                clientConfiguration["DefaultTile"] = clientVisiblePlugins.DefaultPlugin;
                clientConfiguration["Tiles"] = clientVisiblePlugins.Plugins;

                ViewBag.Configuratrion = JsonConvert.SerializeObject(
                    clientConfiguration,
                    new JsonSerializerSettings() { StringEscapeHandling = StringEscapeHandling.Default });

                ViewBag.Templates = (await builder.AggregateStartupAssets()).Templates;
                await builder.UpdateBundles(Bundler.Instance);

                ViewBag.IsAuthenticated = Request.IsAuthenticated ? "true" : "false";
                ViewBag.UserName = HttpContext.User.Identity.Name;

                return this.View();
            }
            catch (Exception exception)
            {
                ViewBag.ErrorMessage = "Could not startup the portal";
                ViewBag.ErrorDetails = exception.Message;
                return this.View("Error");
            }
        }
    }
}