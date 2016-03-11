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
        AdvisorId: ko.observable(""),
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
        Website: ko.observable(""),
        Email: ko.observable(""),
        Password: ko.observable(""),
        PasswordConfirmation: ko.observable(""),
        FirstName: ko.observable(""),
        LastName: ko.observable(""),
        Phone: ko.observable(""),
        Extension: ko.observable(""),
        DomainPrefix: ko.observable(""),
        CreditCardTypes: [
            "Master Card",
            "Visa",
            "American Express",
            "Discover"
        ],
        CardType: ko.observable("Visa"),
        CardHolderName: ko.observable(""),
        CardNumber: ko.observable(""),
        CardExpiryMonth: ko.observable(""),
        CardExpiryYear: ko.observable(""),
        CardCvn: ko.observable(""),
        Month: ko.observable({
            Id: "1",
            Name: "01"
        }),
        Year: ko.observable(2015),
        Months: [
            {
                Id: "1",
                Name: "01"
            },
            {
                Id: "2",
                Name: "02"
            },
            {
                Id: "2",
                Name: "02"
            },
            {
                Id: "3",
                Name: "03"
            },
            {
                Id: "4",
                Name: "04"
            },
            {
                Id: "5",
                Name: "05"
            },
            {
                Id: "6",
                Name: "06"
            },
            {
                Id: "7",
                Name: "07"
            },
            {
                Id: "8",
                Name: "08"
            },
            {
                Id: "9",
                Name: "09"
            },
            {
                Id: "10",
                Name: "10"
            },
            {
                Id: "11",
                Name: "11"
            }
            ,
            {
                Id: "12",
                Name: "12"
            }
        ],
        Years: [
            2015,
            2016,
            2017,
            2018,
            2019,
            2020,
            2021,
            2022
        ]
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