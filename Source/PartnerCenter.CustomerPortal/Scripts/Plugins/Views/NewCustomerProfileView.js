/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Views.NewCustomerProfileView = function (webPortal, elementSelector, isShown, animation) {
    /// <summary>
    /// A view that render input fields for a new customer company and contact information.
    /// </summary>
    /// <param name="webPortal">The web portal instance</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element this view will own.</param>
    /// <param name="isShown">The initial show state. Optional. Default is false.</param>
    /// <param name="animation">Optional animation to use for showing and hiding the view.</param>

    this.base.constructor.call(this, webPortal, elementSelector, isShown, null, animation);
    this.template = "newCustomerProfile-template";

    this.viewModel = {        
        Countries: [
            {
                Id: "US",
                Name: "United States"
            }
        ],
        Country: ko.observable(),
        CompanyName: ko.observable(""),
        AddressLine1: ko.observable(""),
        AddressLine2: ko.observable(""),
        City: ko.observable(""),
        State: ko.observable(""),
        ZipCode: ko.observable(""),        
        Email: ko.observable(""),
        Password: ko.observable(""),
        PasswordConfirmation: ko.observable(""),
        FirstName: ko.observable(""),
        LastName: ko.observable(""),
        Phone: ko.observable(""),        
        DomainPrefix: ko.observable(""),
        CustomerMicrosoftID: ko.observable("") // manage the client id field value. 

    }

    this.viewModel.UserName = ko.computed(function () {
        return "admin@" + this.viewModel.DomainPrefix() + ".onmicrosoft.com";
    }, this);    
}

// extend the base view
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.NewCustomerProfileView, Microsoft.WebPortal.Core.View);

Microsoft.WebPortal.Views.NewCustomerProfileView.prototype.onRender = function () {
    /// <summary>
    /// Called when the view is rendered.
    /// </summary>

    $(this.elementSelector).attr("data-bind", "template: { name: '" + this.template + "'}");
    ko.applyBindings(this, $(this.elementSelector)[0]);    
}

Microsoft.WebPortal.Views.NewCustomerProfileView.prototype.onShowing = function (isShowing) {
}

Microsoft.WebPortal.Views.NewCustomerProfileView.prototype.onShown = function (isShowing) {
}

Microsoft.WebPortal.Views.NewCustomerProfileView.prototype.onDestroy = function () {
    /// <summary>
    /// Called when the journey trail is about to be destroyed.
    /// </summary>

    if ($(this.elementSelector)[0]) {
        // if the element is there, clear its bindings and clean up its content
        ko.cleanNode($(this.elementSelector)[0]);
        $(this.elementSelector).empty();
    }
}

//@ sourceURL=NewCustomerProfileView.js