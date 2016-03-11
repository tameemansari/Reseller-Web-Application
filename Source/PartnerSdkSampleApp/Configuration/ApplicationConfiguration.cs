// -----------------------------------------------------------------------
// <copyright file="ApplicationConfiguration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Configuration;
    using Manager;

    /// <summary>
    /// Abstracts the Web server configuration stored in different places such as web.config and the application and session objects.
    /// </summary>
    public static class ApplicationConfiguration
    {
        /// <summary>
        /// A lazy reference to client configuration.
        /// </summary>
        private static Lazy<IDictionary<string, dynamic>> clientConfiguration = new Lazy<IDictionary<string, dynamic>>(() => ApplicationConfiguration.WebPortalConfigurationManager.GenerateConfigurationDictionary().Result);

        /// <summary>
        /// The web portal configuration file path key.
        /// </summary>
        private static string webPortalConfigurationFilePathKey = "WebPortalConfigurationPath";

        /// <summary>
        /// The web portal configuration manager key.
        /// </summary>
        private static string webPortalConfigurationManagerKey = "WebPortalConfigurationManager";

        /// <summary>
        /// Gets the web portal configuration file path.
        /// </summary>
        public static string WebPortalConfigurationFilePath
        {
            get
            {
                return Path.Combine(
                    HttpRuntime.AppDomainAppPath,
                    WebConfigurationManager.AppSettings[ApplicationConfiguration.webPortalConfigurationFilePathKey]);
            } 
        }

        /// <summary>
        /// Gets the client configuration.
        /// </summary>
        public static IDictionary<string, dynamic> ClientConfiguration
        {
            get
            {
                return clientConfiguration.Value;
            }
        }

        /// <summary>
        /// Gets or sets the web portal configuration manager instance.
        /// </summary>
        public static WebPortalConfigurationManager WebPortalConfigurationManager
        {
            get
            {
                return HttpContext.Current.Application[ApplicationConfiguration.webPortalConfigurationManagerKey] as WebPortalConfigurationManager;
            }

            set
            {
                HttpContext.Current.Application[ApplicationConfiguration.webPortalConfigurationManagerKey] = value;
            }
        }
    }
}