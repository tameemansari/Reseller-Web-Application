/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.Login = function (webPortal) {
    /// <summary>
    /// The Login service. Maintains the login state and broadcasts user login events.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.base.constructor.call(this, webPortal, "Login");

    this.isLoggedIn = ko.observable(false);
    this.isInErrorState = ko.observable(false);
    this.errorMessage = ko.observable();
    this.userName = ko.observable("");
    this.password = ko.observable("");
    this.rememberMe = ko.observable(false);
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.Login, Microsoft.WebPortal.Core.PortalService);

Microsoft.WebPortal.Services.Login.prototype._runService = function () {
    /// <summary>
    /// Runs the Login service.
    /// </summary>
  
    if (isAuthenticated) {
        this.isLoggedIn(true);
        this.userName(userName);
        this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.UserLoggedIn, true);
    }
}

Microsoft.WebPortal.Services.Login.prototype._stopService = function () {
    /// <summary>
    /// Stops the Login service.
    /// </summary>   
}

Microsoft.WebPortal.Services.Login.prototype.login = function () {
    /// <summary>
    /// Displays the login dialog to the user.
    /// </summary>

    var self = this;

    this.webPortal.Services.Dialog.show("login-template", this, [
        Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, "cancelLoginButton", function (owner, button) {
            self.webPortal.Services.Dialog.hide();
        }),
        Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "loginButton", function (owner, button) {
            // ensure the user name and password are specified
            self.isInErrorState(false);

            if (!self.userName()) {
                self.errorMessage("Please specify a user name");
                self.isInErrorState(true);
                return;
            }

            if (!self.password()) {
                self.errorMessage("Please specify a password");
                self.isInErrorState(true);
                return;
            }

            self.webPortal.Services.Dialog.showProgress();

            // send the log in request to the server
            new Microsoft.WebPortal.Utilities.RetryableServerCall(
                self.webPortal.Helpers.ajaxCall("api/Account/Login", Microsoft.WebPortal.HttpMethod.Post, {
                    Email: self.userName(),
                    Password: self.password(),
                    RememberMe: self.rememberMe()
                }), "Login").execute().done(function (result, status, error) {
                    // sign in successful
                    self.isLoggedIn(true);
                    self.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.UserLoggedIn, true);
                    self.webPortal.Services.Dialog.hide();
                }).fail(function (result, status, error) {
                    self.errorMessage(result.statusText);
                    self.isInErrorState(true);
                }).always(function () {
                    self.webPortal.Services.Dialog.hideProgress();
                });
        })
    ]);
}

Microsoft.WebPortal.Services.Login.prototype.logout = function () {
    /// <summary>
    /// Logs the user out from the application.
    /// </summary>

    var self = this;

    // send the log in request to the server
    new Microsoft.WebPortal.Utilities.RetryableServerCall(
        self.webPortal.Helpers.ajaxCall("api/Account/Logout", Microsoft.WebPortal.HttpMethod.Post), "Logout").execute().done(function (result, status, error) {
            // sign out successful
            self.isLoggedIn(false);
            self.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.UserLoggedIn, false);
        }).fail(function (result, status, error) {
        }).always(function () {
        });

    this.isLoggedIn(true);
    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.UserLoggedIn, false);
}


//@ sourceURL=Login.js