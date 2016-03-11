// -----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.App_Start
{
    using System.Web.Http;

    /// <summary>
    /// Configures Web API routes.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers web API routes.
        /// </summary>
        /// <param name="configuration">HTTP configuration.</param>
        public static void Register(HttpConfiguration configuration)
        {
            configuration.MapHttpAttributeRoutes();

            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}
