/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Views.AddSubscriptionsView = function (webPortal, elementSelector, defaultOffer, isShown, animation) {
    /// <summary>
    /// A view that renders UX showing a list of subscriptions to be added from a drop down list.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element this view will own.</param>
    /// <param name="defaultOffer">An optional default offer to add to the subscriptions list.</param>
    /// <param name="isShown">The initial show state. Optional. Default is false.</param>
    /// <param name="animation">Optional animation to use for showing and hiding the view.</param>

    this.base.constructor.call(this, webPortal, elementSelector, isShown, null, animation);
    this.template = "addSubscriptions-template";
    this.offers = this.webPortal.Configuration.Offers;

    var self = this;

    this.selectedOffer = ko.observable();

    // configure the subscriptions list
    this.subscriptionsList = new Microsoft.WebPortal.Views.List(this.webPortal, elementSelector + " #SubscriptionsList", this);

    this.subscriptionsList.setColumns([
        new Microsoft.WebPortal.Views.List.Column("Name", null, false, false, null, null, null, "subscriptionEntry-template")
    ]);

    this.subscriptionsList.showHeader(false);
    this.subscriptionsList.setEmptyListUI("Please add a subscription");
    this.subscriptionsList.enableStatusBar(false);
    this.subscriptionsList.setSelectionMode(Microsoft.WebPortal.Views.List.SelectionMode.None);

    if (defaultOffer) {
        var quantity = ko.observable(1);

        this.subscriptionsList.append([{
            offer: defaultOffer,
            quantity: quantity
        }]);

        quantity.subscribe(function (newValue) {
            if (isNaN(parseInt(newValue))) {
                quantity(1);
            }
        }, this);
    }

    this.onAddOfferClicked = function () {
        var quantity = ko.observable(1);

        self.subscriptionsList.append([{
            offer: self.selectedOffer(),
            quantity: quantity
        }]);

        quantity.subscribe(function (newValue) {
            if (isNaN(parseInt(newValue))) {
                quantity(1);
            }
        }, this);

        $(elementSelector + " #SubscriptionsList").height($(elementSelector + " #SubscriptionsList table").height());
        webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResizing);
        webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResized);
    }

    this.firstPaymentTotal = ko.computed(function () {
        var total = 0;

        for (var i in self.subscriptionsList.rows()) {
            total += self.subscriptionsList.rows()[i].quantity() * self.subscriptionsList.rows()[i].offer.Price;
        }

        return total.toFixed(2);
    });

    this.firstPaymentTotalDisplay = ko.computed(function () {
        return "$" + self.firstPaymentTotal();
    });
}

// extend the base view
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.AddSubscriptionsView, Microsoft.WebPortal.Core.View);

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onRender = function () {
    /// <summary>
    /// Called when the view is rendered.
    /// </summary>

    $(this.elementSelector).attr("data-bind", "template: { name: '" + this.template + "'}");
    ko.applyBindings(this, $(this.elementSelector)[0]);
}

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onShowing = function (isShowing) {
    /// <summary>
    /// Called when the view is about to be shown or hidden.
    /// </summary>
    /// <param name="isShowing">true if showing, false if hiding.</param>

    if (isShowing) {
        this.subscriptionsList.show();
    }
    else {
        this.subscriptionsList.hide();
    }
}

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onShown = function (isShown) {
    /// <summary>
    /// Called when the view is shown or hidden.
    /// </summary>
    /// <param name="isShown">true if shown, false if hidden.</param>

    if (isShown) {
        // resize the list to fit its content
        $(this.elementSelector + " #SubscriptionsList").height($(this.elementSelector + " #SubscriptionsList table").height());

        // force a window resize for the list to resize
        this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResizing);
        this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResized);
    }
}

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onDestroy = function () {
    /// <summary>
    /// Called when the view is about to be destroyed.
    /// </summary>

    if (this.subscriptionsList) {
        this.subscriptionsList.destroy();
    }

    if ($(this.elementSelector)[0]) {
        // if the element is there, clear its bindings and clean up its content
        ko.cleanNode($(this.elementSelector)[0]);
        $(this.elementSelector).empty();
    }
}

//@ sourceURL=AddSubscriptionsView.js