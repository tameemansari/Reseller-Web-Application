// -----------------------------------------------------------------------
// <copyright file="AccountController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using AspNet.Identity.Owin;
    using BusinessLogic;
    using Models;

    /// <summary>
    /// Manages customer login and logouts.
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : BaseController
    {
        /// <summary>
        /// Retrieves a customer account.
        /// </summary>
        /// <returns>The logged in customer account information.</returns>
        [HttpGet]
        [Authorize]
        [Route("")]
        public async Task<CustomerAccountViewModel> CustomerAccount()
        {
            ApplicationUser user = await this.UserManager.FindByNameAsync(HttpContext.Current.User.Identity.Name);
            return await BusinessOperations.GetCustomerAccount(user);
        }

        /// <summary>
        /// Logs a customer in to the application.
        /// </summary>
        /// <param name="loginViewModel">The login information.</param>
        /// <returns>A HTTP response message of 200 if successful. Otherwise, it will correspond to the error.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<HttpResponseMessage> Login(LoginViewModel loginViewModel)
        {
            var response = new HttpResponseMessage();

            if (!ModelState.IsValid)
            {
                response.StatusCode = HttpStatusCode.BadRequest;

                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();

                response.ReasonPhrase = errorList[0];
            }
            else
            {
                var result = await this.SignInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, shouldLockout: false);

                switch (result)
                {
                    case SignInStatus.Success:
                        response.StatusCode = HttpStatusCode.OK;
                        break;
                    case SignInStatus.LockedOut:
                        response.StatusCode = HttpStatusCode.Forbidden;
                        response.ReasonPhrase = "This account has been locked out. Please try again later.";
                        break;
                    default:
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        response.ReasonPhrase = "Invalid login attempt.";
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Logs a user out of the application.
        /// </summary>
        [HttpPost]
        public void LogOut()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut();
        }
    }
}