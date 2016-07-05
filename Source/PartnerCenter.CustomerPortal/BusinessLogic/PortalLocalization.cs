// -----------------------------------------------------------------------
// <copyright file="PortalLocalization.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic
{
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration;

    /// <summary>
    /// Encapsulates locale information for the portal based on the partner's region.
    /// </summary>
    public class PortalLocalization : DomainObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalLocalization"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public PortalLocalization(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Gets the portal's country ISO2 code. E.g. US
        /// </summary>
        public string CountryIso2Code { get; private set; }

        /// <summary>
        /// Gets the portal's locale. E.g. En-US
        /// </summary>
        public string Locale { get; private set; }

        /// <summary>
        /// Gets the portal's ISO currency code. E.g. USD
        /// </summary>
        public string CurrencyCode { get; private set; }

        /// <summary>
        /// Gets the portal's currency symbol. E.g. $
        /// </summary>
        public string CurrencySymbol { get; private set; }

        /// <summary>
        /// Initializes state and ensures the object is ready to be consumed.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task InitializeAsync()
        {
            var partnerLegalBusinessProfile = await this.ApplicationDomain.PartnerCenterClient.Profiles.LegalBusinessProfile.GetAsync();
            this.CountryIso2Code = partnerLegalBusinessProfile.Address.Country;

            // figure out the currency
            var partnerRegion = new RegionInfo(this.CountryIso2Code);
            this.CurrencyCode = partnerRegion.ISOCurrencySymbol;
            this.CurrencySymbol = partnerRegion.CurrencySymbol;

            // get the locale, we default to the first locale used in a country for now
            var partnerCountryValidationRules = await ApplicationDomain.Instance.PartnerCenterClient.CountryValidationRules.ByCountry(this.CountryIso2Code).GetAsync();
            this.Locale = partnerCountryValidationRules.SupportedCulturesList.FirstOrDefault();
        }
    }
}