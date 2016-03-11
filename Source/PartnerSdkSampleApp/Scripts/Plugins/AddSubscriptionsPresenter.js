/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.AddSubscriptionsPresenter = function (webPortal, feature, context) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "Home", "/Template/AddSubscriptions/");

    this.addSubscriptionsView = new Microsoft.WebPortal.Views.AddSubscriptionsView(webPortal, "#AddSubscriptionsViewContainer", context);

    this.onCancelClicked = function () {
        webPortal.Journey.retract();
    }

    var self = this;
    var isPosting = false;

    this.getOrders = function () {
        var orders = [];

        for (var i in this.addSubscriptionsView.subscriptionsList.rows()) {
            orders.push({
                OfferId: this.addSubscriptionsView.subscriptionsList.rows()[i].offer.Id,
                FriendlyName: this.addSubscriptionsView.subscriptionsList.rows()[i].offer.Title,
                Quantity: this.addSubscriptionsView.subscriptionsList.rows()[i].quantity()
            });
        }

        return orders;
    }

    this.onSubmitClicked = function () {
        if (isPosting) {
            return;
        }

        if (self.addSubscriptionsView.subscriptionsList.rows().length <= 0 || self.addSubscriptionsView.firstPaymentTotal() <= 0) {
            self.webPortal.Services.Dialog.show("emptyOffersErrorMessage-template", {}, [
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "ok-Button", function () {
                    self.webPortal.Services.Dialog.hide();
                })
            ]);

            return;
        }

        isPosting = true;
        var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress, "Adding subscriptions");
        self.webPortal.Services.Notifications.add(notification);

        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/Subscription", Microsoft.WebPortal.HttpMethod.Post, self.getOrders(), Microsoft.WebPortal.ContentType.Json, 120000), "AddSubscriptions", []).execute().done(function () {
            notification.dismiss();
            self.webPortal.Journey.start(Microsoft.WebPortal.Feature.CustomerAccount);

        }).fail(function (result, status, error) {
            notification.message(error);
            notification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
            notification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, "Retry", function () {
                    notification.dismiss();
                    self.onSubmitClicked();
                }),
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, "Cancel", function () {
                    notification.dismiss();
                })
            ]);
        }).always(function () {
            isPosting = false;
        });
    }
}

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.AddSubscriptionsPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.AddSubscriptionsPresenter.prototype.onActivate = function () {
    /// <summary>
    /// Called when the presenter is activated.
    /// </summary>
}

Microsoft.WebPortal.AddSubscriptionsPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    ko.applyBindings(this, $("#Form")[0]);

    this.addSubscriptionsView.render();
}

Microsoft.WebPortal.AddSubscriptionsPresenter.prototype.onShow = function () {
    /// <summary>
    /// Called when content is shown.
    /// </summary>

    this.addSubscriptionsView.show();
}

//@ sourceURL=AddSubscriptionsPresenter.js