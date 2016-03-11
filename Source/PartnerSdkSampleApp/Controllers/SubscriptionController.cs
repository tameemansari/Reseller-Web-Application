// -----------------------------------------------------------------------
// <copyright file="SubscriptionController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using BusinessLogic;
    using Models;
    using PartnerCenter.Models.Subscriptions;

    /// <summary>
    /// Manages customer subscriptions.
    /// </summary>
    [RoutePrefix("api/Subscription")]
    public class SubscriptionController : BaseController
    {
        /// <summary>
        /// Retrieves a customer's subscriptions.
        /// </summary>
        /// <returns>The customer subscriptions.</returns>
        [Authorize]
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<Subscription>> GetSubscriptions()
        {
            ApplicationUser user = await this.UserManager.FindByNameAsync(HttpContext.Current.User.Identity.Name);
            return await BusinessOperations.GetCustomerSubscriptions(user.CustomerId);
        }

        /// <summary>
        /// Adds new subscriptions to a customer.
        /// </summary>
        /// <param name="subscriptions">The new subscriptions.</param>
        /// <returns>An HTTP response indicating the result of the addition.</returns>
        [Authorize]
        [HttpPost]
        [Route("")]
        public async Task AddSubscriptions(IEnumerable<OrderViewModel> subscriptions)
        {
            ApplicationUser user = await this.UserManager.FindByNameAsync(HttpContext.Current.User.Identity.Name);
            await BusinessOperations.PlaceOrder(user.CustomerId, subscriptions);
        }

        /// <summary>
        /// Updates a customer's subscriptions.
        /// </summary>
        /// <param name="subscriptions">A list of subscriptions to update.</param>
        /// <returns>The updated subscriptions.</returns>
        [Authorize]
        [HttpPut]
        [Route("")]
        public async Task<IEnumerable<Subscription>> UpdateSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            ApplicationUser user = await this.UserManager.FindByNameAsync(HttpContext.Current.User.Identity.Name);
            return await BusinessOperations.UpdateSubscriptions(user.CustomerId, subscriptions);
        }
    }
}
