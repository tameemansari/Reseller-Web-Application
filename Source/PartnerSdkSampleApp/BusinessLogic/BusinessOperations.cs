// -----------------------------------------------------------------------
// <copyright file="BusinessOperations.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerApplication.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Web;
    using AspNet.Identity.Owin;
    using Configuration;
    using Models;
    using PartnerCenter;
    using PartnerCenter.Models;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Offers;
    using PartnerCenter.Models.Orders;
    using PartnerCenter.Models.Subscriptions;

    /// <summary>
    /// Holds the business logic of the application.
    /// </summary>
    public static class BusinessOperations
    {
        /// <summary>
        /// A reference to partner operations.
        /// </summary>
        private static IAggregatePartner partnerOperations = HttpContext.Current.Application["PartnerOperations"] as IAggregatePartner;

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="customerViewModel">The customer view model.</param>
        /// <returns>The newly created customer.</returns>
        public static async Task<Customer> CreateCustomer(CustomerViewModel customerViewModel)
        {
            Customer newCustomer = new Customer()
            {
                CompanyProfile = new CustomerCompanyProfile()
                {
                    Domain = string.Format(CultureInfo.InvariantCulture, "{0}.onmicrosoft.com", customerViewModel.DomainPrefix),
                },
                BillingProfile = new CustomerBillingProfile()
                {
                    Culture = "EN-US",
                    Language = "En",
                    Email = customerViewModel.Email,
                    CompanyName = customerViewModel.CompanyName,

                    DefaultAddress = new Address()
                    {
                        FirstName = customerViewModel.FirstName,
                        LastName = customerViewModel.LastName,
                        AddressLine1 = customerViewModel.AddressLine1,
                        AddressLine2 = customerViewModel.AddressLine2,
                        City = customerViewModel.City,
                        State = customerViewModel.State,
                        Country = customerViewModel.Country,
                        PostalCode = customerViewModel.ZipCode,
                        PhoneNumber = customerViewModel.Phone,
                    }
                }
            };

            return await partnerOperations.Customers.CreateAsync(newCustomer);
        }

        /// <summary>
        /// Gets a customer's subscriptions.
        /// </summary>
        /// <param name="customerId">The customer Id.</param>
        /// <returns>The customer's subscriptions.</returns>
        public static async Task<IEnumerable<Subscription>> GetCustomerSubscriptions(string customerId)
        {
            return (await partnerOperations.Customers.ById(customerId).Subscriptions.GetAsync()).Items;
        }

        /// <summary>
        /// Places an order for a customer.
        /// </summary>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="orderLineItems">A list of order line items.</param>
        /// <returns>The created order.</returns>
        public static async Task<Order> PlaceOrder(string customerId, IEnumerable<OrderViewModel> orderLineItems)
        {
            var lineItems = new List<OrderLineItem>();

            int lineItemNumber = 0;

            foreach (var orderLineItem in orderLineItems)
            {
                lineItems.Add(new OrderLineItem()
                {
                    LineItemNumber = lineItemNumber++,
                    OfferId = orderLineItem.OfferId,
                    FriendlyName = orderLineItem.FriendlyName,
                    Quantity = orderLineItem.Quantity
                });
            }

            var order = new Order()
            {
                ReferenceCustomerId = customerId,
                LineItems = lineItems
            };

            return await partnerOperations.Customers.ById(customerId).Orders.CreateAsync(order);
        }

        /// <summary>
        /// Retrieves the customer account for a user.
        /// </summary>
        /// <param name="user">An application user.</param>
        /// <returns>The tied customer account for this user.</returns>
        public static async Task<CustomerAccountViewModel> GetCustomerAccount(ApplicationUser user)
        {
            var subscriptions = await partnerOperations.Customers.ById(user.CustomerId).Subscriptions.GetAsync();

            List<SubscriptionViewModel> subscriptionViewModels = new List<SubscriptionViewModel>();

            foreach (var subscription in subscriptions.Items)
            {
                var offer = await subscription.Links.Offer.InvokeAsync<Offer>(partnerOperations);

                subscriptionViewModels.Add(new SubscriptionViewModel()
                {
                    FriendlyName = subscription.FriendlyName,
                    Quantity = subscription.Quantity.ToString(),
                    Price = GetOfferPrice(offer.Id)
                });
            }

            return new CustomerAccountViewModel()
            {
                Subscriptions = subscriptionViewModels
            };
        }

        /// <summary>
        /// Builds a registration confirmation for a new customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="registrationInformation">The registration confirmation.</param>
        /// <returns>The registration confirmation view model.</returns>
        public static async Task<RegistrationConfirmationViewModel> GetRegistrationConfirmation(Customer customer, CustomerRegistrationViewModel registrationInformation)
        {
            var subscriptions = await partnerOperations.Customers.ById(customer.CompanyProfile.TenantId).Subscriptions.GetAsync();

            List<SubscriptionViewModel> subscriptionViewModels = new List<SubscriptionViewModel>();

            foreach (var subscription in subscriptions.Items)
            {
                var offer = await subscription.Links.Offer.InvokeAsync<Offer>(partnerOperations);

                subscriptionViewModels.Add(new SubscriptionViewModel()
                {
                    FriendlyName = subscription.FriendlyName,
                    Quantity = subscription.Quantity.ToString(),
                    Price = GetOfferPrice(offer.Id)
                });
            }

            var user = new ApplicationUser { UserName = registrationInformation.Customer.Email, Email = registrationInformation.Customer.Email, CustomerId = customer.CompanyProfile.TenantId };
            var result = await HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().UpdateAsync(user);

            return new RegistrationConfirmationViewModel()
            {
                AdvisorId = registrationInformation.Customer.AdvisorId,
                AddressLine1 = customer.BillingProfile.DefaultAddress.AddressLine1,
                AddressLine2 = customer.BillingProfile.DefaultAddress.AddressLine2,
                City = customer.BillingProfile.DefaultAddress.City,
                State = customer.BillingProfile.DefaultAddress.State,
                ZipCode = customer.BillingProfile.DefaultAddress.PostalCode,
                Country = customer.BillingProfile.DefaultAddress.Country,
                Phone = customer.BillingProfile.DefaultAddress.PhoneNumber,
                Language = customer.BillingProfile.Language,
                FirstName = customer.BillingProfile.DefaultAddress.FirstName,
                LastName = customer.BillingProfile.DefaultAddress.LastName,
                CreditCardNumber = "xxxx xxxx xxxx " + registrationInformation.Customer.CreditCardNumber.Substring(11),
                CreditCardExpiry = string.Format("{0}/{1}", registrationInformation.Customer.CreditCardExpiryMonth, registrationInformation.Customer.CreditCardExpiryYear),
                CreditCardType = registrationInformation.Customer.CreditCardType,
                Email = customer.BillingProfile.Email,
                CompanyName = customer.BillingProfile.CompanyName,
                MicrosoftId = customer.CompanyProfile.TenantId,
                UserName = registrationInformation.Customer.Email,
                Subscriptions = subscriptionViewModels,
                Website = registrationInformation.Customer.Website,
                AdminUserAccount = customer.UserCredentials.UserName + "@" + customer.CompanyProfile.Domain
            };
        }

        /// <summary>
        /// Registers a new user based on a customer view model.
        /// </summary>
        /// <param name="customerViewModel">The customer view model.</param>
        /// <returns>A new application user.</returns>
        internal static async Task<ApplicationUser> RegisterUser(CustomerViewModel customerViewModel)
        {
            var user = new ApplicationUser { UserName = customerViewModel.Email, Email = customerViewModel.Email };
            var result = await HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().CreateAsync(user, customerViewModel.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(new List<string>(result.Errors)[0]);
            }

            return user;
        }

        /// <summary>
        /// Updates an application user with the given customer tenant Id.
        /// </summary>
        /// <param name="user">The application user.</param>
        /// <param name="customerTenantId">The customer tenant Id.</param>
        /// <returns>The updated application user.</returns>
        internal static async Task<ApplicationUser> UpdateUser(ApplicationUser user, string customerTenantId)
        {
            user.CustomerId = customerTenantId;
            var result = await HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(new List<string>(result.Errors)[0]);
            }

            return user;
        }

        /// <summary>
        /// Updates a customer's subscriptions.
        /// </summary>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="subscriptions">A list of subscriptions.</param>
        /// <returns>The updated subscriptions.</returns>
        internal static async Task<IEnumerable<Subscription>> UpdateSubscriptions(string customerId, IEnumerable<Subscription> subscriptions)
        {
            var updatedSubscriptions = new List<Subscription>();

            foreach (var subscription in subscriptions)
            {
                updatedSubscriptions.Add(await partnerOperations.Customers.ById(customerId).Subscriptions.ById(subscription.Id).PatchAsync(subscription));
            }

            return updatedSubscriptions;
        }

        /// <summary>
        /// Retrieves the configured price of an offer.
        /// </summary>
        /// <param name="offerId">The offer Id.</param>
        /// <returns>The offer price.</returns>
        private static string GetOfferPrice(string offerId)
        {
            var configuredOffers = ApplicationConfiguration.ClientConfiguration["Offers"];
            string offerPrice = "0";

            foreach (var offer in configuredOffers)
            {
                if (offer.Id == offerId)
                {
                    offerPrice = offer.Price;
                    break;
                }
            }

            return offerPrice;
        }
    }
}