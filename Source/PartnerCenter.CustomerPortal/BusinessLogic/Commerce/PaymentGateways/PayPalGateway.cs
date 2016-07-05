// -----------------------------------------------------------------------
// <copyright file="PayPalGateway.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce.PaymentGateways
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;
    using PayPal;
    using PayPal.Api;    

    /// <summary>
    /// PayPal payment gateway implementation.
    /// </summary>
    public class PayPalGateway : DomainObject, IPaymentGateway
    {
        /// <summary>
        /// Maintains the credit card against which the payment is being managed. 
        /// </summary>
        private readonly Models.PaymentCard creditCard;

        /// <summary>
        /// Maintains the description for this payment. 
        /// </summary>
        private readonly string paymentDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayPalGateway" /> class. 
        /// </summary>
        /// <param name="applicationDomain">The ApplicationDomain</param>
        /// <param name="paymentCard">The Payment Card used in this commerce operation.</param>
        /// <param name="description">The description which will be added to the Payment Card authorization call.</param>
        public PayPalGateway(ApplicationDomain applicationDomain, Models.PaymentCard paymentCard, string description) : base(applicationDomain)
        {
            paymentCard.AssertNotNull(nameof(paymentCard));
            description.AssertNotEmpty(nameof(description));

            // clean up credit card number input. 
            string cardNumber = paymentCard.CreditCardNumber;
            cardNumber = cardNumber.Replace(" ", string.Empty); // remove empty spaces in the string. 
            cardNumber = cardNumber.Replace("-", string.Empty); // remove dashes.
            paymentCard.CreditCardNumber = cardNumber;

            this.creditCard = paymentCard;
            this.paymentDescription = description;
        }

        /// <summary>
        /// Validates payment configuration. 
        /// </summary>
        /// <param name="paymentConfig">The Payment configuration.</param>
        public static void ValidateConfiguration(PaymentConfiguration paymentConfig)
        {
            string[] supportedPaymentModes = { "sandbox", "live" };

            paymentConfig.AssertNotNull(nameof(paymentConfig));

            paymentConfig.ClientId.AssertNotEmpty(nameof(paymentConfig.ClientId));
            paymentConfig.ClientSecret.AssertNotEmpty(nameof(paymentConfig.ClientSecret));
            paymentConfig.AccountType.AssertNotEmpty(nameof(paymentConfig.AccountType));

            if (!supportedPaymentModes.Contains(paymentConfig.AccountType))
            {
                throw new PartnerDomainException("Payment mode is not supported");
            }

            try
            {
                Dictionary<string, string> configMap = new Dictionary<string, string>();
                configMap.Add("clientId", paymentConfig.ClientId);
                configMap.Add("clientSecret", paymentConfig.ClientSecret);
                configMap.Add("mode", paymentConfig.AccountType);

                string accessToken = new OAuthTokenCredential(configMap).GetAccessToken();
                var apiContext = new APIContext(accessToken);
            }
            catch (PayPalException paypalException)
            {                
                if (paypalException is IdentityException)
                {
                    // thrown when API Context couldn't be setup. 
                    IdentityException identityFailure = paypalException as IdentityException;
                    IdentityError failureDetails = identityFailure.Details;
                    if (failureDetails != null && failureDetails.error.ToLower() == "invalid_client")
                    {                        
                        throw new PartnerDomainException(ErrorCode.PaymentGatewayIdentityFailureDuringConfiguration).AddDetail("ErrorMessage", Resources.PaymentGatewayIdentityFailureDuringConfiguration);
                    }                    
                }

                // if this is not an identity exception rather some other issue. 
                throw new PartnerDomainException(ErrorCode.PaymentGatewayFailure).AddDetail("ErrorMessage", paypalException.Message);                            
            }
        }

        /// <summary>
        /// Authorizes a payment with PayPal. Ensures the payment is valid.
        /// </summary>
        /// <param name="amount">The amount to charge.</param>
        /// <returns>An authorization code.</returns>
        public async Task<string> AuthorizeAsync(decimal amount)
        {
            amount.AssertPositive(nameof(amount));

            CreditCard customerCard = new CreditCard();
            customerCard.first_name = this.creditCard.CardHolderFirstName;
            customerCard.last_name = this.creditCard.CardHolderLastName;
            customerCard.expire_month = this.creditCard.CreditCardExpiryMonth;
            customerCard.expire_year = this.creditCard.CreditCardExpiryYear;
            customerCard.number = this.creditCard.CreditCardNumber;
            customerCard.type = this.creditCard.CreditCardType;
            customerCard.cvv2 = this.creditCard.CreditCardCvn;

            //// PayPal expects the following in the amount (string)input during authorization... 
            //// There are a few currencies where decimals are not supported. For future consideration. 
            //// Currency amount must be 
            ////      non - negative number, 
            ////      may optionally contain exactly 2 decimal places separated by '.', 
            ////      optional thousands separator ',', 
            ////      limited to 7 digits before the decimal point], 
            ////      Conversion needs to be done since Commerce layer treats it as a decimal while PayPal expects a string. 
            ////      "12.0"(ie - without the additional zero...expected would be "12.00"). https://msdn.microsoft.com/en-us/library/dwhawy9k(v=vs.110).aspx#FFormatString             

            string currency = this.ApplicationDomain.PortalLocalization.CurrencyCode;
            string orderAmt = amount.ToString("F", CultureInfo.InvariantCulture);             

            // Create Payment Object. 
            var payment = new Payment()
            {
                // Intent is to Authroize this Payment. 
                intent = "authorize",                           // required
                // Payer Object (Required)
                // A resource representing a Payer that funds a payment. Use the List of `FundingInstrument` and the Payment Method as 'credit_card'
                payer = new Payer()
                {
                    payment_method = "credit_card",              // required                 
                    // funding instruments object []. 
                    // The Payment creation API requires a list of
                    // FundingInstrument; add the created `FundingInstrument` to a List
                    funding_instruments = new List<FundingInstrument>()
                    {
                        // A resource representing a Payeer's funding instrument. and the `CreditCardDetails`
                        new FundingInstrument()
                        {
                            // A resource representing a credit card that can be used to fund a payment.
                            credit_card = customerCard
                        }
                    },
                },
                
                // transactions object []. (Required)
                // The Payment creation API requires a list of transactions; add the created `Transaction` to a List
                transactions = new List<Transaction>()
                {                                      
                    // A transaction defines the contract of a payment - what is the payment for and who is fulfilling it. Transaction is created with a `Payee` and `Amount` types                    
                    new Transaction()
                    {
                        // Let's you specify a payment amount.
                        amount = new Amount()
                        {                            
                            currency = currency,    // Required
                            total = orderAmt        // Required
                        },
                        description = this.paymentDescription                        
                    }                    
                }                                                                
            };

            // Create a payment by posting to the APIService using a valid APIContext
            // Retrieve an Authorization Id by making a Payment with intent as 'authorize' and parsing through the Payment object
            Payment createdPayment = null;
            try
            {
                createdPayment = payment.Create(await this.GetAPIContext());
            }
            catch (PayPalException ex)
            {
                this.ParsePayPalException(ex);
            }              

            return await Task.FromResult(createdPayment.transactions[0].related_resources[0].authorization.id);
        }

        /// <summary>
        /// Finalizes an authorized payment with PayPal.
        /// </summary>
        /// <param name="authorizationCode">The authorization code for the payment to capture.</param>
        /// <returns>A task.</returns>
        public async Task CaptureAsync(string authorizationCode)
        {
            string authorizationCurrency;
            string authorizationAmount;
            Authorization cardAuthorization = null;

            authorizationCode.AssertNotEmpty(nameof(authorizationCode));
            
            APIContext apiContext = await this.GetAPIContext();

            // given the authorizationId. Lookup the authorization to find the amount. 
            try
            {
                cardAuthorization = Authorization.Get(apiContext, authorizationCode);
                authorizationCurrency = cardAuthorization.amount.currency;
                authorizationAmount = cardAuthorization.amount.total;

                // Setting 'is_final_capture' to true, all remaining funds held by the authorization will be released from the funding instrument.
                var capture = new Capture()
                {
                    amount = new Amount()
                    {
                        currency = authorizationCurrency,
                        total = authorizationAmount
                    },
                    is_final_capture = true
                };

                var responseCapture = cardAuthorization.Capture(apiContext, capture);
                await Task.FromResult(string.Empty);
            }
            catch (PayPalException ex)
            {
                this.ParsePayPalException(ex);
            }
        }

        /// <summary>
        /// Voids an authorized payment with PayPal.
        /// </summary>
        /// <param name="authorizationCode">The authorization code for the payment to void.</param>
        /// <returns>a Task</returns>
        public async Task VoidAsync(string authorizationCode)
        {
            authorizationCode.AssertNotEmpty(nameof(authorizationCode));

            // given the authorizationId string... Lookup the authorization to void it. 
            try
            {                
                APIContext apiContext = await this.GetAPIContext();
                Authorization cardAuthorization = Authorization.Get(apiContext, authorizationCode);
                cardAuthorization.Void(apiContext);
                await Task.FromResult(string.Empty);
            }
            catch (PayPalException ex)
            {
                this.ParsePayPalException(ex);
            }
        }

        /// <summary>
        /// Retrieves the API Context for PayPal. 
        /// </summary>
        /// <returns>PayPal APIContext</returns>
        private async Task<APIContext> GetAPIContext()
        {
            //// The GetAccessToken() of the SDK Returns the currently cached access token. 
            //// If no access token was previously cached, or if the current access token is expired, then a new one is generated and returned. 
            //// See more - https://github.com/paypal/PayPal-NET-SDK/blob/develop/Source/SDK/Api/OAuthTokenCredential.cs

            // Before getAPIContext ... set up PayPal configuration. This is an expensive call which can benefit from caching. 
            PaymentConfiguration paymentConfig = await ApplicationDomain.Instance.PaymentConfigurationRepository.RetrieveAsync();

            Dictionary<string, string> configMap = new Dictionary<string, string>();
            configMap.Add("clientId", paymentConfig.ClientId);
            configMap.Add("clientSecret", paymentConfig.ClientSecret);
            configMap.Add("mode", paymentConfig.AccountType);

            string accessToken = new OAuthTokenCredential(configMap).GetAccessToken();
            var apiContext = new APIContext(accessToken);
            apiContext.Config = configMap;
            return apiContext;
        }

        /// <summary>
        /// Throws PartnerDomainException by parsing PayPal exception. 
        /// </summary>
        /// <param name="ex">Exceptions from PayPal SDK.</param>        
        private void ParsePayPalException(PayPalException ex)
        {
            if (ex is PaymentsException)
            {
                PaymentsException pe = ex as PaymentsException;

                // Get the details of this exception with ex.Details and format the error message in the form of "We are unable to process your payment –  {Errormessage} :: [err1, err2, .., errN]".                
                StringBuilder errorString = new StringBuilder();
                errorString.Append(Resources.PaymentGatewayErrorPrefix);                
                
                if (pe.Details != null)
                {
                    string errorName = pe.Details.name.ToUpper();

                    if (errorName == null || errorName.Length < 1)
                    {
                        errorString.Append(pe.Details.message);
                        throw new PartnerDomainException(ErrorCode.PaymentGatewayFailure).AddDetail("ErrorMessage", errorString.ToString());
                    }                        

                    // build error string for errors returned from financial institutions.
                    if (errorName.Contains("CREDIT_CARD_REFUSED"))
                    {
                        throw new PartnerDomainException(ErrorCode.CardRefused);                        
                    }
                    else if (errorName.Contains("EXPIRED_CREDIT_CARD"))
                    {
                        throw new PartnerDomainException(ErrorCode.CardExpired);
                    }
                    else if (errorName.Contains("CREDIT_CARD_CVV_CHECK_FAILED"))
                    {                        
                        throw new PartnerDomainException(ErrorCode.CardCVNCheckFailed);
                    }
                    else if (errorName.Contains("UNKNOWN_ERROR"))
                    {                        
                        throw new PartnerDomainException(ErrorCode.PaymentGatewayPaymentError);
                    }
                    else if (errorName.Contains("VALIDATION") && pe.Details.details != null)
                    {
                        // Check if there are sub collection details and build error string.                                       
                        errorString.Append("[");
                        foreach (ErrorDetails errorDetails in pe.Details.details)
                        {
                            // removing extrataneous information.                     
                            string errorField = errorDetails.field;
                            if (errorField.Contains("payer.funding_instruments[0]."))
                            {
                                errorField = errorField.Replace("payer.funding_instruments[0].", string.Empty).ToString();
                            }

                            errorString.AppendFormat("{0} - {1},", errorField, errorDetails.issue);
                        }

                        errorString.Replace(',', ']', errorString.Length - 2, 2); // remove the last comma and replace it with ]. 
                    }
                    else
                    {
                        errorString.Append(Resources.UseAlternateCardMessage);
                    }
                }

                throw new PartnerDomainException(ErrorCode.PaymentGatewayFailure).AddDetail("ErrorMessage", errorString.ToString());
            }

            if (ex is IdentityException)
            {
                // ideally this shouldn't be raised from customer experience calls. 
                // can occur when admin has generated a new secret for an existing app id in PayPal but didnt update portal payment configuration.                                
                throw new PartnerDomainException(ErrorCode.PaymentGatewayIdentityFailureDuringPayment).AddDetail("ErrorMessage", Resources.PaymentGatewayIdentityFailureDuringPayment);
            }

            // few PayPalException types contain meaningfull exception information only in InnerException. 
            if (ex is PayPalException && ex.InnerException != null)
            {                
                throw new PartnerDomainException(ErrorCode.PaymentGatewayFailure).AddDetail("ErrorMessage", ex.InnerException.Message);
            }
            else
            {                
                throw new PartnerDomainException(ErrorCode.PaymentGatewayFailure).AddDetail("ErrorMessage", ex.Message);
            }
        }
    }
}