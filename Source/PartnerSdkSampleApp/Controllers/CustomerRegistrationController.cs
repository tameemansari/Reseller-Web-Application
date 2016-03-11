// -----------------------------------------------------------------------
// <copyright file="CustomerRegistrationController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using BusinessLogic;
    using Models;
    using Newtonsoft.Json;
    using PartnerCenter.Exceptions;

    /// <summary>
    /// Manages new customer registration.
    /// </summary>
    [RoutePrefix("api/CustomerRegistration")]
    public class CustomerRegistrationController : BaseController
    {
        /// <summary>
        /// Registers a new customer and placed an order for them.
        /// </summary>
        /// <param name="customerRegistrationInformation">The customer's registration information.</param>
        /// <returns>A registration confirmation.</returns>
        [HttpPost]
        public async Task<RegistrationConfirmationViewModel> Register([FromBody] CustomerRegistrationViewModel customerRegistrationInformation)
        {
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();

                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = JsonConvert.SerializeObject(errorList);

                throw new HttpResponseException(response);
            }

            try
            {
                var user = await BusinessOperations.RegisterUser(customerRegistrationInformation.Customer);
                var newCustomer = await BusinessOperations.CreateCustomer(customerRegistrationInformation.Customer);
                user = await BusinessOperations.UpdateUser(user, newCustomer.CompanyProfile.TenantId);
                var order = await BusinessOperations.PlaceOrder(newCustomer.CompanyProfile.TenantId, customerRegistrationInformation.Orders);
                return await BusinessOperations.GetRegistrationConfirmation(newCustomer, customerRegistrationInformation);
            }
            catch (PartnerException partnerException)
            {
                HttpResponseMessage errorResponse = new HttpResponseMessage();
                errorResponse.ReasonPhrase = partnerException.ServiceErrorPayload.ErrorMessage;

                switch (partnerException.ErrorCategory)
                {
                    case PartnerErrorCategory.BadInput:
                        errorResponse.StatusCode = HttpStatusCode.BadRequest;
                        break;
                    case PartnerErrorCategory.Unauthorized:
                        errorResponse.StatusCode = HttpStatusCode.Unauthorized;
                        break;
                    default:
                        errorResponse.StatusCode = HttpStatusCode.InternalServerError;
                        break;
                }

                throw new HttpResponseException(errorResponse);
            }
            catch (InvalidOperationException userCreateProblem)
            {
                HttpResponseMessage errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                errorResponse.ReasonPhrase = userCreateProblem.Message;

                throw new HttpResponseException(errorResponse);
            }
        }
    }
}
