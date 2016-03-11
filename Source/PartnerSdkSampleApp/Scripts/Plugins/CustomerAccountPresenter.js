/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.CustomerAccountPresenter = function (webPortal, feature) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "CustomerAccount", "/Template/CustomerAccount/");

    this.onAddSubscriptionsClicked = function () {
        // go to the add subscriptions page
        webPortal.Journey.advance(Microsoft.WebPortal.Feature.AddSubscriptions);
    }

    this.onUpdateSubscriptionsClicked = function () {
        // go to the update subscriptions page
        webPortal.Journey.advance(Microsoft.WebPortal.Feature.UpdateSubscriptions);
    }
}

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.CustomerAccountPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.CustomerAccountPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    var self = this;
    self.webPortal.ContentPanel.showProgress();

    var getCustomerAccountServerCall =
        this.webPortal.ServerCallManager.create(this.feature, self.webPortal.Helpers.ajaxCall("api/Account/", Microsoft.WebPortal.HttpMethod.Get), "GetCustomerAccount");

    getCustomerAccountServerCall.execute().done(function (customerAccount) {
        self.viewModel = customerAccount;

        self.viewModel.TotalPrice = 0

        if (self.viewModel.Subscriptions) {
            for (var i in self.viewModel.Subscriptions) {
                self.viewModel.Subscriptions[i].PriceDisplay = "$" + self.viewModel.Subscriptions[i].Price;
                self.viewModel.TotalPrice += parseFloat(self.viewModel.Subscriptions[i].Price) * self.viewModel.Subscriptions[i].Quantity;
            }
        }

        self.viewModel.TotalPrice = "$" + self.viewModel.TotalPrice.toFixed(2);

        ko.applyBindings(self, $("#CustomerAccountContainer")[0]);
    }).fail(function (result, status, error) {
        var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error, "Could not retrieve customer account");

        notification.buttons([
            Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, "Retry", function () {
                notification.dismiss();
                self.onActivate();
            }),
            Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, "Cancel", function () {
                notification.dismiss();
            })
        ]);

        self.webPortal.Services.Notifications.add(notification);
    }).always(function () {
        self.webPortal.ContentPanel.hideProgress();
    });
}

//@ sourceURL=CustomerAccountPresenter.js