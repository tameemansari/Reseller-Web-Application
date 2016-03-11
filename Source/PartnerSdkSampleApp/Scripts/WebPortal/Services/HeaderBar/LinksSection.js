/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.LinksSection = function (webPortal) {
    /// <summary>
    /// Renders the links the header bar.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.base.constructor.call(this, webPortal, "LinksSection", "linksHeaderBarSection-template");
    this.style("padding:0; width: 1px;");

    this.isLoggedIn = ko.observable(false);

    this.onLoginClicked = function () {
        webPortal.Services.Login.login();
    }

    webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.UserLoggedIn, function (eventId, isLoggedIn, broadcaster) {
        this.isLoggedIn(isLoggedIn);

        if (isLoggedIn) {
            webPortal.Services.HeaderBar.addSection(new Microsoft.WebPortal.Services.UserSection(webPortal));

            webPortal.Services.UserMenu.add(new Microsoft.WebPortal.Services.Action("SignOut", "Sign Out", function () {
                // Implement sign out
                webPortal.Services.Login.logout();
            }, null, "Sign out from the application"));
        } else {
            webPortal.Services.HeaderBar.removeSection("UserInfoSection");
        }
    }, this);
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.LinksSection, Microsoft.WebPortal.Services.HeaderBarSection);

//@ sourceURL=HelpSection.js