// -----------------------------------------------------------------------
// <copyright file="PartnerOfferController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using BusinessLogic;
    using Models;

    /// <summary>
    /// Serves partner offers to callers.
    /// </summary>
    [RoutePrefix("api/partnerOffers")]
    public class PartnerOfferController : BaseController
    {
        /// <summary>
        /// Retrieves all the active offers the partner has configured.
        /// </summary>
        /// <returns>The active partner offers.</returns>
        [Route("")]
        [HttpGet]
        public async Task<OfferCatalogViewModel> GetOffersCatalog()
        {
            var isBrandingConfigured = ApplicationDomain.Instance.PortalBranding.IsConfiguredAsync();
            var isOffersConfigured = ApplicationDomain.Instance.OffersRepository.IsConfiguredAsync();
            var isPaymentConfigured = ApplicationDomain.Instance.PaymentConfigurationRepository.IsConfiguredAsync();

            var getMicrosoftOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveMicrosoftOffersAsync();
            var getPartnerOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveAsync();

            await Task.WhenAll(isBrandingConfigured, isOffersConfigured, isPaymentConfigured);

            var offerCatalogViewModel = new OfferCatalogViewModel();
            offerCatalogViewModel.IsPortalConfigured = isBrandingConfigured.Result && isOffersConfigured.Result && isPaymentConfigured.Result;

            if (offerCatalogViewModel.IsPortalConfigured)
            {
                await Task.WhenAll(getMicrosoftOffersTask, getPartnerOffersTask);

                var microsoftOffers = getMicrosoftOffersTask.Result;
                var partnerOffers = getPartnerOffersTask.Result.Where(offer => offer.IsInactive == false);

                foreach (var offer in partnerOffers)
                {
                    offer.Thumbnail = microsoftOffers.Where(msOffer => msOffer.Offer.Id == offer.MicrosoftOfferId).First().ThumbnailUri;
                }

                offerCatalogViewModel.Offers = partnerOffers;
            }

            return offerCatalogViewModel;
        }
    }
}