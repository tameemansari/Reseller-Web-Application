/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Core.SessionManager = function (webPortal) {
    /// <summary>
    /// Stores session information.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.webPortal = webPortal;

    // Shell, please let us know when you have finished initializing
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.PortalInitializing, this.initialize, this);

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.UserLoggedIn, this.onUserLoginChanged, this);

    // a hashtable that caches the HTML template for each feature
    this.featureTemplates = {};
};

Microsoft.WebPortal.Core.SessionManager.prototype.initialize = function (eventId, context, broadcaster) {
    /// <summary>
    /// Called when the portal is initializing.
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="context"></param>
    /// <param name="broadcaster"></param>

    // assign feature presenters
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.Offers, Microsoft.WebPortal.OffersPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.CustomerRegistration, Microsoft.WebPortal.CustomerRegistrationPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.RegistrationConfirmation, Microsoft.WebPortal.RegistrationConfirmationPresenter);

    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.CustomerAccount, Microsoft.WebPortal.CustomerAccountPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.AddSubscriptions, Microsoft.WebPortal.AddSubscriptionsPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.UpdateSubscriptions, Microsoft.WebPortal.UpdateSubscriptionsPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.UpdateContactInformation, Microsoft.WebPortal.UpdateContactInformationPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.UpdateCompanyInformation, Microsoft.WebPortal.UpdateCompanyInformationPresenter);
}

Microsoft.WebPortal.Core.SessionManager.prototype.onUserLoginChanged = function (eventId, isLoggedIn, broadcaster) {
    /// <summary>
    /// Called when the user is logged in or out.
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="context"></param>
    /// <param name="broadcaster"></param>

    // update the account tile hidden status
    this.webPortal.tiles()[1].Hidden = !isLoggedIn;

    // refresh the header bar
    this.webPortal.Services.PrimaryNavigation.stop();
    this.webPortal.Services.PrimaryNavigation.run();
}

//@ sourceURL=SessionManager.js
