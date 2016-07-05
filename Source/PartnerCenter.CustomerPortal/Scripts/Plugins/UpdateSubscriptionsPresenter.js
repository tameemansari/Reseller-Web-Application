/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.UpdateSubscriptionsPresenter = function (webPortal, feature, subscriptionItem) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    /// <param name="subscriptionItem">The customer subscription item which is being edited.</param>

    this.base.constructor.call(this, webPortal, feature, "Update Subscriptions", "/Template/UpdateSubscriptions/");
    this.creditCardInputView = new Microsoft.WebPortal.Views.CreditCardInputView(webPortal, "#CreditCardInputContainer");

    var self = this;
    self.isPosting = false;    

    // these viewModel fields are used to control how the form elements behave. 
    // viewModel.isUpdateSubscription is true when form is for updating an existing subscription. 
    // viewModel.isRenewSubscription  is true when form is for renewing an existing subscription.
    
    this.viewModel = subscriptionItem;

    // set up quantity field in the form. 
    self.viewModel.Quantity = ko.observable(1);
    if (self.viewModel.isRenewSubscription) {
        self.viewModel.Quantity(self.viewModel.LicensesTotal);
    }

    // setting up page title based on whether page is loaded to renew or add seats to a subscription. 
    this.viewModel.PageTitle = "";
    if (this.viewModel.isUpdateSubscription) {
        this.viewModel.PageTitle = self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.AddMoreSeatsTitleText;
    } else if (this.viewModel.isRenewSubscription) {
        this.viewModel.PageTitle = self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.RenewSubscriptionTitleText;
    }


    this.viewModel.ShowProgress = ko.observable(true);
    this.viewModel.IsSet = ko.observable(false); 

    // form event handlers follow. 
    this.onCancelClicked = function () {
        webPortal.Journey.retract();
    }

    this.onSubmitClicked = function () {
        if (self.isPosting) {
            return;
        }

        self.webPortal.Services.Notifications.clear();
        var subscriptions = self.viewModel;        

        if ($("#Form").valid()) {            
            self.isPosting = true;
            this.raiseOrder();
        } else {
            // the form is invalid            
        }
    }

    this.raiseOrder = function () {
        /// <summary>
        /// Called to renew or add more seats to the subscription.  
        /// </summary>
        var thisNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress, self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.UpdatingSubscriptionMessage);
        self.webPortal.Services.Notifications.add(thisNotification);

        if (self.viewModel.isUpdateSubscription)
            apiUrl = "api/Subscription/AddSeats";

        if (self.viewModel.isRenewSubscription)
            apiUrl = "api/Subscription/Renew";

        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall(apiUrl, Microsoft.WebPortal.HttpMethod.Post, {
            Subscriptions: this.getSubscriptions(),
            CreditCard: this.getCreditCardInfo()            
        }, Microsoft.WebPortal.ContentType.Json, 120000), apiUrl, []).execute()        
        .done(function () {
            // hand it off to the subscriptions page.        
            thisNotification.dismiss();
            self.webPortal.Journey.start(Microsoft.WebPortal.Feature.Subscriptions);
        })        
        .fail(function (result, status, error) {                        
            thisNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
            thisNotification.buttons([                
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, self.webPortal.Resources.Strings.OK, function () {
                    thisNotification.dismiss();
                })
            ]);

            var errorPayload = JSON.parse(result.responseText);

            if (errorPayload) {
                switch (errorPayload.ErrorCode) {
                    case Microsoft.WebPortal.ErrorCode.InvalidInput:
                        thisNotification.message(self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.InvalidInputErrorPrefix + errorPayload.Details.ErrorMessage);
                        break;
                    case Microsoft.WebPortal.ErrorCode.DownstreamServiceError:
                        thisNotification.message(self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.DownstreamErrorPrefix + errorPayload.Details.ErrorMessage);
                        break;
                    case Microsoft.WebPortal.ErrorCode.CardCVNCheckFailed:
                        thisNotification.message(self.webPortal.Resources.Strings.Plugins.CreditCardView.PaymentGatewayErrorPrefix + self.webPortal.Resources.Strings.Plugins.CreditCardView.CardCVNFailedError);
                        break;
                    case Microsoft.WebPortal.ErrorCode.CardExpired:                        
                        thisNotification.message(self.webPortal.Resources.Strings.Plugins.CreditCardView.PaymentGatewayErrorPrefix + self.webPortal.Resources.Strings.Plugins.CreditCardView.CardExpiredError + self.webPortal.Resources.Strings.Plugins.CreditCardView.UseAlternateCardMessage);
                        break;
                    case Microsoft.WebPortal.ErrorCode.CardRefused:
                        thisNotification.message(self.webPortal.Resources.Strings.Plugins.CreditCardView.PaymentGatewayErrorPrefix + self.webPortal.Resources.Strings.Plugins.CreditCardView.CardRefusedError +self.webPortal.Resources.Strings.Plugins.CreditCardView.UseAlternateCardMessage);
                        break;
                    case Microsoft.WebPortal.ErrorCode.PaymentGatewayPaymentError:
                        thisNotification.message(self.webPortal.Resources.Strings.Plugins.CreditCardView.PaymentGatewayErrorPrefix + self.webPortal.Resources.Strings.Plugins.CreditCardView.UseAlternateCardMessage);
                        break;
                    case Microsoft.WebPortal.ErrorCode.PaymentGatewayIdentityFailureDuringPayment:
                    case Microsoft.WebPortal.ErrorCode.PaymentGatewayFailure:
                        thisNotification.message(errorPayload.Details.ErrorMessage);
                        break;
                    default:
                        thisNotification.message(self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.OrderAddFailureMessage);
                        break;
                }
            } else {
                thisNotification.message(self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.OrderUpdateFailureMessage);                
            }



        })
        .always(function () {
            self.isPosting = false;
        });
    }

    this.getSubscriptions = function () {
        var orders = [];
        
        orders.push({
            SubscriptionId: self.viewModel.SubscriptionId,  // required for the add seats & renew calls.             
            Quantity: self.viewModel.Quantity()             // this.addSubscriptionsView.subscriptionsList.rows()[i].quantity()
        });
        
        return orders;
    }


    this.getCreditCardInfo = function () {
        var paymentCard = {
            CreditCardType: this.creditCardInputView.viewModel.CardType(),
            CardHolderFirstName: this.creditCardInputView.viewModel.CardHolderFirstName(),
            CardHolderLastName: this.creditCardInputView.viewModel.CardHolderLastName(),
            CreditCardNumber: this.creditCardInputView.viewModel.CardNumber(),
            CreditCardExpiryMonth: this.creditCardInputView.viewModel.Month(),
            CreditCardExpiryYear: this.creditCardInputView.viewModel.Year(),
            CreditCardCvn: this.creditCardInputView.viewModel.CardCvn()
        }

        return paymentCard;
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
    // show credit card control after fetching subscriptions. 
    this.creditCardInputView.render();    

    var self = this;

    var getPortalOfferDetails = function () {
        self.viewModel.IsSet(false);
        var portaloffersFetchProgress = $.Deferred();
        self.webPortal.Session.fetchPortalOffers(portaloffersFetchProgress);
        portaloffersFetchProgress.done(function (portalOffers) {
            // find & set up the portal offer from the cached offers in the Portal. 
            var matchedOffer = ko.utils.arrayFirst(self.webPortal.Session.IdMappedPortalOffers, function (item) {
                return item.Id.toLowerCase() === self.viewModel.PortalOfferId.toLowerCase();
            });
            self.viewModel.portalOffer = matchedOffer.OriginalOffer;

            // pricePerSeat - Manages the price per seat. Will either be normal offer price or pro rated price provided by server. 
            self.viewModel.pricePerSeat = 0;
            if (self.viewModel.isUpdateSubscription) {
                self.viewModel.pricePerSeat = self.viewModel.SubscriptionProRatedPrice.toFixed(2);
            }
            if (self.viewModel.isRenewSubscription) {
                self.viewModel.pricePerSeat = self.viewModel.portalOffer.Price.toFixed(2);
            }

            // set up the total charge form field computed value. 
            self.viewModel.TotalCharge = ko.computed(function () {
                var total = 0;
                total = self.viewModel.Quantity() * self.viewModel.pricePerSeat;
                return total.toFixed(2);
            });
            self.viewModel.IsSet(true);
        }).fail(function (result, status, error) {
            var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error,
                self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.CouldNotRetrieveOffer);

            notification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, self.webPortal.Resources.Strings.Retry, function () {
                    notification.dismiss();
                    getPortalOfferDetails();
                }),
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, self.webPortal.Resources.Strings.Cancel, function () {
                    notification.dismiss();
                })
            ]);

            self.webPortal.Services.Notifications.add(notification);
        }).always(function () {
            self.viewModel.ShowProgress(false);
        });
    }

    getPortalOfferDetails();
}

Microsoft.WebPortal.UpdateSubscriptionsPresenter.prototype.onShow = function () {
    /// <summary>
    /// Called when content is shown.
    /// </summary>
    
    this.creditCardInputView.show();    
}



//@ sourceURL=UpdateSubscriptionsPresenter.js