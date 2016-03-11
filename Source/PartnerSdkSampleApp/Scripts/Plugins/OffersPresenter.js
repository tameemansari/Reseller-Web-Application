/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.OffersPresenter = function (webPortal, feature) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "Home", "/Template/Offers/");

    this.offers = this.webPortal.Configuration.Offers.slice(0, 3);

    this.onBuyNowClicked = function (offer) {
        // activate the customer registration presenter and pass it the selected offer
        webPortal.Journey.advance(Microsoft.WebPortal.Feature.CustomerRegistration, offer);
    }
}

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.OffersPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.OffersPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is rendered but not shown yet.
    /// </summary>

    ko.applyBindings(this, $("#OffersContainer")[0]);
}

//@ sourceURL=OffersPresenter.js