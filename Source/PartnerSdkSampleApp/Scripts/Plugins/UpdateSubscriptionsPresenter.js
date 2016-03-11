/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.UpdateSubscriptionsPresenter = function (webPortal, feature, context) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "Home", "/Template/UpdateSubscriptions/");

    this.viewModel = ko.observable([]);

    this.onCancelClicked = function () {
        webPortal.Journey.retract();
    }

    var self = this;
    var isPosting = false;

    this.onSubmitClicked = function () {
        if (isPosting) {
            return;
        }

        self.webPortal.Services.Notifications.clear();

        var subscriptions = self.viewModel();
        var errorMessage = "";

        for (var i in subscriptions) {
            if (isNaN(parseInt(subscriptions[i].Quantity())) || subscriptions[i].Quantity() < 1) {
                errorMessage = "Please enter a number of licences which is a number greater than zero";
                break;
            } else if (subscriptions[i].FriendlyName().length <= 0) {
                errorMessage = "Subscription alias can't be empty";
                break;
            }
        }

        if (errorMessage) {
            var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error, errorMessage);

            notification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "Ok", function () {
                    notification.dismiss();
                })
            ]);

            self.webPortal.Services.Notifications.add(notification);

            return;
        }

        var payload = [];

        for (var i in subscriptions) {
            subscriptions[i].Quantity = subscriptions[i].Quantity();
            subscriptions[i].FriendlyName = subscriptions[i].FriendlyName();
            subscriptions[i].Status = subscriptions[i].Status();

            payload.push(subscriptions[i]);
        }

        isPosting = true;
        var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress, "Updating subscriptions");
        self.webPortal.Services.Notifications.add(notification);

        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/Subscription", Microsoft.WebPortal.HttpMethod.Put, payload,
            Microsoft.WebPortal.ContentType.Json, 120000), "UpdateSubscriptions", []).execute().done(function (updatedSubscriptions) {
            notification.dismiss();
            self.webPortal.Journey.retract();
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
$WebPortal.Helpers.inherit(Microsoft.WebPortal.UpdateSubscriptionsPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.UpdateSubscriptionsPresenter.prototype.onActivate = function () {
    /// <summary>
    /// Called when the presenter is activated.
    /// </summary>
}

Microsoft.WebPortal.UpdateSubscriptionsPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    ko.applyBindings(this, $("#Form")[0]);

    var self = this;
    self.webPortal.ContentPanel.showProgress();

    new Microsoft.WebPortal.Utilities.RetryableServerCall(self.webPortal.Helpers.ajaxCall("api/Subscription", Microsoft.WebPortal.HttpMethod.Get), "GetSubscriptions", []).execute().done(function (subscriptions) {
        for (var i in subscriptions) {
            for(var j in self.webPortal.Configuration.Offers) {
                if (self.webPortal.Configuration.Offers[j].Id === subscriptions[i].OfferId) {
                    subscriptions[i].offer = self.webPortal.Configuration.Offers[j];
                    break;
                }
            }

            subscriptions[i].Quantity = ko.observable(subscriptions[i].Quantity);
            subscriptions[i].FriendlyName = ko.observable(subscriptions[i].FriendlyName);
            subscriptions[i].Status = ko.observable(subscriptions[i].Status);
            subscriptions[i].Name = "subscription" + subscriptions[i].Id;
        }

        self.viewModel(subscriptions);
    }).fail(function (result, status, error) {
        var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error, "Could not retrieve customer subscriptions");

        notification.buttons([
            Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, "Retry", function () {
                notification.dismiss();
                self.onRender();
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

Microsoft.WebPortal.UpdateSubscriptionsPresenter.prototype.onShow = function () {
    /// <summary>
    /// Called when content is shown.
    /// </summary>
}

//@ sourceURL=UpdateSubscriptionsPresenter.js