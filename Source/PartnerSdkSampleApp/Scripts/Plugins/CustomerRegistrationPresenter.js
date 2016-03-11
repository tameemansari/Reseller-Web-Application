/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.CustomerRegistrationPresenter = function (webPortal, feature, context) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "Home", "/Template/CustomerRegistration/");

    this.addSubscriptionsView = new Microsoft.WebPortal.Views.AddSubscriptionsView(webPortal, "#AddSubscriptionsViewContainer", context);
    this.customerProfileView = new Microsoft.WebPortal.Views.NewCustomerProfileView(webPortal, "#CustomerProfileContainer");

    this.context = context;

    var self = this;
    var isPosting = false;

    this.onFormSubmit = function () {
        if (isPosting) {
            return;
        }

        //var cannedResponse = {
        //    AdvisorId: "SampleAdvisor",
        //    AddressLine1: "17020 NE 2nd PL",
        //    AddressLine2: "Home",
        //    City: "Bellevue",
        //    State: "WA",
        //    ZipCode: "98008",
        //    Country: "United States",
        //    Phone: "2065965577",
        //    Language: "English",
        //    FirstName: "Raed",
        //    LastName: "Jarrar",
        //    CreditCardNumber: "xxxx xxxx xxxx 1234",
        //    CreditCardExpiry: "12/20",
        //    CreditCardType: "AMEX",
        //    Email: "raed788@yahoo.com",
        //    CompanyName: "RaedSoft",
        //    MicrosoftId: "65476541a6545564654",
        //    UserName: "raed788@yahoo.com",
        //    Subscriptions: [
        //        {
        //            FriendlyName: "Office 1",
        //            Quantity: "1",
        //            Price: "14.99"
        //        },
        //        {
        //            FriendlyName: "Office 2",
        //            Quantity: "2",
        //            Price: "35.99"
        //        }
        //    ],
        //    Website: "www.raed.com",
        //    AdminUserAccount: "raed@alphacorp.ccstp.com"
        //}

        //self.webPortal.Journey.advance(Microsoft.WebPortal.Feature.RegistrationConfirmation, cannedResponse);
        //return;

        if ($("#Form").valid()) {
            if (self.addSubscriptionsView.subscriptionsList.rows().length <= 0 || self.addSubscriptionsView.firstPaymentTotal() <= 0) {
                self.webPortal.Services.Dialog.show("emptyOffersErrorMessage-template", {}, [
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "ok-Button", function () {
                        self.webPortal.Services.Dialog.hide();
                    })
                ]);

                return;
            }

            isPosting = true;
            var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress, "Registering customer and placing the order");
            self.webPortal.Services.Notifications.add(notification);

            new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/CustomerRegistration", Microsoft.WebPortal.HttpMethod.Post, {
                Orders: this.getOrders(),
                Customer: this.getCustomerInformation()
            }, Microsoft.WebPortal.ContentType.Json, 120000), "RegisterCustomer", []).execute().done(function (registrationConfirmation) {
                notification.dismiss();

                // hand it off to the registration summary presenter
                self.webPortal.Journey.advance(Microsoft.WebPortal.Feature.RegistrationConfirmation, registrationConfirmation);

            }).fail(function (result, status, error) {
                notification.message(error);
                notification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
                notification.buttons([
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, "Retry", function () {
                        notification.dismiss();
                        self.onFormSubmit();
                    }),
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, "Cancel", function () {
                        notification.dismiss();
                    })
                ]);
            }).always(function () {
                isPosting = false;
            });
        } else {
            // the form is invalid
        }
    }

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

    this.getCustomerInformation = function () {
        var customerInformation = {
            AdvisorId: this.customerProfileView.viewModel.AdvisorId(),
            Country: this.customerProfileView.viewModel.Country(),
            CompanyName: this.customerProfileView.viewModel.CompanyName(),
            AddressLine1: this.customerProfileView.viewModel.AddressLine1(),
            AddressLine2: this.customerProfileView.viewModel.AddressLine2(),
            City: this.customerProfileView.viewModel.City(),
            State: this.customerProfileView.viewModel.State(),
            ZipCode: this.customerProfileView.viewModel.ZipCode(),
            Website: this.customerProfileView.viewModel.Website(),
            Email: this.customerProfileView.viewModel.Email(),
            Password: this.customerProfileView.viewModel.Password(),
            PasswordConfirmation: this.customerProfileView.viewModel.PasswordConfirmation(),
            FirstName: this.customerProfileView.viewModel.FirstName(),
            LastName: this.customerProfileView.viewModel.LastName(),
            Phone: this.customerProfileView.viewModel.Phone(),
            Extension: this.customerProfileView.viewModel.Extension(),
            DomainPrefix: this.customerProfileView.viewModel.DomainPrefix(),
            CreditCardType: this.customerProfileView.viewModel.CardType(),
            CreditCardHolderName: this.customerProfileView.viewModel.CardHolderName(),
            CreditCardNumber: this.customerProfileView.viewModel.CardNumber(),
            CreditCardExpiryMonth: this.customerProfileView.viewModel.Month(),
            CreditCardExpiryYear: this.customerProfileView.viewModel.Year(),
            CreditCardCvn: this.customerProfileView.viewModel.CardCvn()
        }

        return customerInformation;
    }
}

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.CustomerRegistrationPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.CustomerRegistrationPresenter.prototype.onActivate = function () {
    /// <summary>
    /// Called when the presenter is activated.
    /// </summary>
}

Microsoft.WebPortal.CustomerRegistrationPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    ko.applyBindings(this, $("#Form")[0]);

    this.addSubscriptionsView.render();
    this.customerProfileView.render();
}

Microsoft.WebPortal.CustomerRegistrationPresenter.prototype.onShow = function () {
    /// <summary>
    /// Called when content is shown.
    /// </summary>

    this.addSubscriptionsView.show();
    this.customerProfileView.show();
}

//@ sourceURL=CustomerRegistrationPresenter.js