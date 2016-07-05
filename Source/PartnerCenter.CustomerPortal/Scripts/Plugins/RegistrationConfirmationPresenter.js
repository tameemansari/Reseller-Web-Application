/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.RegistrationConfirmationPresenter = function (webPortal, feature, registrationConfirmationViewModel) {
    /// <summary>
    /// Shows the registration confirmation page.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    /// <param name="registrationConfirmationViewModel">The registration confirmation view model.</param>
    this.base.constructor.call(this, webPortal, feature, "Home", "/Template/RegistrationConfirmation/");

    var self = this;
    self.viewModel = registrationConfirmationViewModel;
    self.viewModel.Subscriptions = registrationConfirmationViewModel.CreatedSubscriptions.Subscriptions;
    self.viewModel.TotalPrice = registrationConfirmationViewModel.CreatedSubscriptions.SummaryTotal;

    var addressLine = this.viewModel.AddressLine1;

    if (this.viewModel.AddressLine2) {
        addressLine += " " + this.viewModel.AddressLine2;
    }

    this.viewModel.Address = [
        addressLine,
        this.viewModel.City + ", " + this.viewModel.State + " " + this.viewModel.ZipCode,
        this.viewModel.Country
    ];

    this.viewModel.ContactInformation = [
        this.viewModel.FirstName + " " + this.viewModel.LastName,
        this.viewModel.Email,
        this.viewModel.Phone
    ];

    this.onDoneClicked = function () {
        // go back to the home page
        webPortal.Journey.start(Microsoft.WebPortal.Feature.Home);        
    }
}

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.RegistrationConfirmationPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.RegistrationConfirmationPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    ko.applyBindings(this, $("#RegistrationConfirmationContainer")[0]);

}

//@ sourceURL=RegistrationConfirmationPresenter.js