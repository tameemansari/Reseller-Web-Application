// -----------------------------------------------------------------------
// <copyright file="GraphClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Graph;
    using Models;
    using Security;

    /// <summary>
    /// Provides the ability to interact with the Microsoft Graph.
    /// </summary>
    /// <seealso cref="IGraphClient" />
    public class GraphClient : IGraphClient
    {
        /// <summary>
        /// Static instance of the <see cref="HttpProvider" /> class.
        /// </summary>
        private static HttpProvider httpProvider = new HttpProvider(new HttpClientHandler(), false);

        /// <summary>
        /// Provides access to the Microsoft Graph.
        /// </summary>
        private readonly IGraphServiceClient client;

        /// <summary>
        /// Identifier of the customer.
        /// </summary>
        private readonly string customerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphClient"/> class.
        /// </summary>
        /// <param name="customerId">Identifier for customer whose resources are being accessed.</param>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// or
        /// <paramref name="authorizationCode"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="redirectUri"/> is null.
        /// </exception>"
        public GraphClient(string customerId, string authorizationCode, Uri redirectUri)
        {
            customerId.AssertNotEmpty(nameof(customerId));
            authorizationCode.AssertNotEmpty(nameof(authorizationCode));

            this.customerId = customerId;

            AuthenticationProvider authProvider = new AuthenticationProvider(customerId, authorizationCode, redirectUri);
            client = new GraphServiceClient(authProvider, httpProvider);
        }


        /// <summary>
        /// Gets a list of roles assigned to the specified object identifier.
        /// </summary>
        /// <param name="objectId">Object identifier for the object to be checked.</param>
        /// <returns>A list of roles that that are associated with the specified object identifier.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="objectId"/> is empty or null.
        /// </exception>
        public async Task<List<RoleModel>> GetDirectoryRolesAsync(string objectId)
        {
            DateTime executionTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            IUserMemberOfCollectionWithReferencesPage directoryGroups;
            List<RoleModel> roles;
            List<DirectoryRole> directoryRoles;
            bool morePages;

            objectId.AssertNotEmpty(nameof(objectId));

            try
            {
                executionTime = DateTime.Now;

                directoryGroups = await client.Users[objectId].MemberOf.Request().GetAsync().ConfigureAwait(false);
                roles = new List<RoleModel>();

                do
                {
                    directoryRoles = directoryGroups.CurrentPage.OfType<DirectoryRole>().ToList();

                    if (directoryRoles.Count > 0)
                    {
                        roles.AddRange(directoryRoles.Select(r => new RoleModel
                        {
                            Description = r.Description,
                            DisplayName = r.DisplayName
                        }));
                    }

                    morePages = directoryGroups.NextPageRequest != null;

                    if (morePages)
                    {
                        directoryGroups = await directoryGroups.NextPageRequest.GetAsync().ConfigureAwait(false);
                    }
                }
                while (morePages);

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", customerId },
                    { "ObjectId", objectId }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(executionTime).TotalMilliseconds },
                    { "NumberOfRoles", roles.Count }
                };

                ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent(nameof(GetDirectoryRolesAsync), eventProperties, eventMeasurements);

                // await VerifyTeamsLicenseAssignment(objectId).ConfigureAwait(false);

                try
                {
                    // for the passed in userObjectId assign 
                    bool alreadyAssigned = false;
                    string m365skuId = "cbdc14ab-d96c-4c30-b9f4-6ada7cdc1d46";

                    var response1 = await client.Users[objectId].LicenseDetails.Request().GetAsync().ConfigureAwait(false);
                    if (response1.Count >= 0)
                    {
                        foreach (LicenseDetails ld in response1)
                        {
                            string tocheck = ld.SkuId.ToString();
                            if (m365skuId.Equals(tocheck, StringComparison.OrdinalIgnoreCase))
                            {
                                alreadyAssigned = true;
                                break;
                            }
                        }
                    }

                    if (!alreadyAssigned)
                    {
                        AssignedLicense aLicense = new AssignedLicense { SkuId = new Guid(m365skuId) };
                        IList<AssignedLicense> licensesToAdd = new AssignedLicense[] { aLicense };
                        IList<Guid> licensesToRemove = Array.Empty<Guid>();

                        await client.Users[objectId].AssignLicense(licensesToAdd, licensesToRemove).Request().PostAsync().ConfigureAwait(false);
                    }

                }
                catch (Exception)
                {
                    // do nothing 
                }

                return roles;
            }
            catch (Exception ex)
            {
                ApplicationDomain.Instance.TelemetryService.Provider.TrackException(ex);
                return null;
            }
        }

        public async Task<bool> VerifyTeamsLicenseAssignment(string objectId)
        {
            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("InVerifyLicense");

            var userInfo = await client.Users[objectId].Request().GetAsync().ConfigureAwait(false);
            // var skus = await client.SubscribedSkus.Request().GetAsync();

            // for the passed in userObjectId assign 
            bool alreadyAssigned = false;
            string m365skuId = "cbdc14ab-d96c-4c30-b9f4-6ada7cdc1d46";

            var response1 = await client.Users[userInfo.Id].LicenseDetails.Request().GetAsync().ConfigureAwait(false);
            if (response1.Count >= 0)
            {
                foreach (LicenseDetails ld in response1)
                {
                    string tocheck = ld.SkuId.ToString();
                    if (m365skuId.Equals(tocheck, StringComparison.OrdinalIgnoreCase))                    
                    {
                        alreadyAssigned = true;
                        break;
                    }
                }
            }

            if (alreadyAssigned) return true;
            else
            {
                AssignedLicense aLicense = new AssignedLicense { SkuId = new Guid(m365skuId) };
                IList<AssignedLicense> licensesToAdd = new AssignedLicense[] { aLicense };
                IList<Guid> licensesToRemove = Array.Empty<Guid>();

                await client.Users[objectId].AssignLicense(licensesToAdd, licensesToRemove).Request().PostAsync().ConfigureAwait(false);                
            }

            return true;

        }
    }
}