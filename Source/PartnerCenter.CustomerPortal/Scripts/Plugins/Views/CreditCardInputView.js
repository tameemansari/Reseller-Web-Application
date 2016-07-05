/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Views.CreditCardInputView = function (webPortal, elementSelector, isShown, animation) {
    /// <summary>
    /// A view that render input fields for Credit Card information.
    /// </summary>
    /// <param name="webPortal">The web portal instance</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element this view will own.</param>
    /// <param name="isShown">The initial show state. Optional. Default is false.</param>
    /// <param name="animation">Optional animation to use for showing and hiding the view.</param>

    this.base.constructor.call(this, webPortal, elementSelector, isShown, null, animation);
    this.template = "CreditCardInput-template";

    this.monthsArray = [];
    for (counter = 1; counter <= 12; ++counter) {
        this.monthsArray.push(counter);
    }

    this.currentYear = new Date().getFullYear();
    this.yearsArray = [];
    for (counter = this.currentYear; counter < this.currentYear + 10; ++counter) {
        this.yearsArray.push(counter);
    }
    
    this.currentMonth = new Date().getMonth() + 1;

    this.viewModel = {        
        CardHolderFirstName: ko.observable(""),
        CardHolderLastName: ko.observable(""),
        CardNumber: ko.observable(""),
        CardExpiryMonth: ko.observable(""),        
        CardExpiryYear: ko.observable(""),
        CardCvn: ko.observable(""),
        Year: ko.observable(this.currentYear),
        Month: ko.observable(this.currentMonth)
    }

    this.viewModel.Months = this.monthsArray;
    this.viewModel.Years = this.yearsArray;

    this.viewModel.CardTypeCaption = ko.computed(function () {
        var self = this;
        number = this.viewModel.CardNumber();

        // Regex Borrowed from https://github.com/jzaefferer/jquery-validation/blob/master/src/additional/creditcardtypes.js
        // if nothing is entered. 
        if (number.length < 1) {                    
            self.viewModel.CardType = ko.observable("unknown");
            return "";
        }
        // check for Visa card. 
        else if (/^(4)/.test(number)) {
            self.viewModel.CardType = ko.observable("visa");
            return " - " + self.webPortal.Resources.Strings.Plugins.CreditCardView.CreditCardVisaCaption;
        }
        // check for master card. 
        else if (/^(5[12345])/.test(number)) {    
            self.viewModel.CardType = ko.observable("mastercard");
            return " - " + self.webPortal.Resources.Strings.Plugins.CreditCardView.CreditCardMasterCaption;            
        }
        // check for Amex card. 
        else if (/^(3[47])/.test(number)) {       
            self.viewModel.CardType = ko.observable("amex");
            return " - " + self.webPortal.Resources.Strings.Plugins.CreditCardView.CreditCardAmexCaption;            
        }
        // check for Discover card. 
        else if (/^(6011)/.test(number)) {        
            self.viewModel.CardType = ko.observable("discover");
            return " - " + self.webPortal.Resources.Strings.Plugins.CreditCardView.CreditCardDiscoverCaption;
        }
        // default is unknown card. 
        else {                                    
            self.viewModel.CardType = ko.observable("unknown");
            return " - " + self.webPortal.Resources.Strings.Plugins.CreditCardView.CreditCardNotSupportedCaption;
        }
    }, this);


}

// extend the base view
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.CreditCardInputView, Microsoft.WebPortal.Core.View);

Microsoft.WebPortal.Views.CreditCardInputView.prototype.onRender = function () {
    /// <summary>
    /// Called when the view is rendered.
    /// </summary>

    $(this.elementSelector).attr("data-bind", "template: { name: '" + this.template + "'}");
    ko.applyBindings(this, $(this.elementSelector)[0]);
}

Microsoft.WebPortal.Views.CreditCardInputView.prototype.onShowing = function (isShowing) {
}

Microsoft.WebPortal.Views.CreditCardInputView.prototype.onShown = function (isShowing) {
}

Microsoft.WebPortal.Views.CreditCardInputView.prototype.onDestroy = function () {
    /// <summary>
    /// Called when the journey trail is about to be destroyed.
    /// </summary>

    if ($(this.elementSelector)[0]) {
        // if the element is there, clear its bindings and clean up its content
        ko.cleanNode($(this.elementSelector)[0]);
        $(this.elementSelector).empty();
    }
}

//@ sourceURL=CreditCardInputView.js