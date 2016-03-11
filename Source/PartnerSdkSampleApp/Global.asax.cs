// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication
{
    using System.Configuration;
    using System.Data.Entity;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using App_Start;
    using Configuration;
    using Configuration.Manager;
    using Extensions;
    using Models;
    using PartnerCenter;

    /// <summary>
    /// The web application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Called when the application starts.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            Database.SetInitializer<ApplicationDbContext>(null);

            string portalConfigurationPath = ApplicationConfiguration.WebPortalConfigurationFilePath;

            if (string.IsNullOrWhiteSpace(portalConfigurationPath))
            {
                throw new ConfigurationErrorsException("WebPortalConfigurationPath setting not found in web.config");
            }

            IWebPortalConfigurationFactory webPortalConfigFactory = new WebPortalConfigurationFactory();
            ApplicationConfiguration.WebPortalConfigurationManager = webPortalConfigFactory.Create(portalConfigurationPath);

            var credentials = PartnerCredentials.Instance.GenerateByApplicationCredentials(
                ConfigurationManager.AppSettings["aad.applicationId"],
                ConfigurationManager.AppSettings["aad.applicationSecret"],
                ConfigurationManager.AppSettings["aad.applicationDomain"],
                ConfigurationManager.AppSettings["aad.authority"],
                ConfigurationManager.AppSettings["aad.graphEndpoint"]);

            this.Application["PartnerOperations"] = PartnerService.Instance.CreatePartnerOperations(credentials);
        }
    }
}
