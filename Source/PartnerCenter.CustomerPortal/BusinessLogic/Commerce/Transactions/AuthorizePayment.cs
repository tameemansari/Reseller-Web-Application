// -----------------------------------------------------------------------
// <copyright file="AuthorizePayment.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Infrastructure;

    /// <summary>
    /// Authorizes a payment with a payment gateway.
    /// </summary>
    public class AuthorizePayment : IBusinessTransactionWithOutput<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizePayment"/> class.
        /// </summary>
        /// <param name="paymentGateway">The payment gateway to use for authorization.</param>
        /// <param name="amountToCharge">The amount to charge.</param>
        public AuthorizePayment(IPaymentGateway paymentGateway, decimal amountToCharge)
        {
            paymentGateway.AssertNotNull(nameof(paymentGateway));
            amountToCharge.AssertPositive(nameof(amountToCharge));

            this.Amount = amountToCharge;
            this.PaymentGateway = paymentGateway;
        }

        /// <summary>
        /// Gets the payment gateway used for authorization.
        /// </summary>
        public IPaymentGateway PaymentGateway { get; private set; }

        /// <summary>
        /// Gets the amount to charge.
        /// </summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// Gets the authorization code.
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// Authorizes the payment amount.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            // authorize with the payment gateway
            this.Result = await this.PaymentGateway.AuthorizeAsync(this.Amount);
        }

        /// <summary>
        /// Rolls back the authorization.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            if (!string.IsNullOrWhiteSpace(this.Result))
            {
                try
                {
                    // void the previously authorized payment
                    await this.PaymentGateway.VoidAsync(this.Result);
                }
                catch (Exception voidingProblem)
                {
                    if (voidingProblem.IsFatal())
                    {
                        throw;
                    }

                    Trace.TraceError("AuthorizePayment.RollbackAsync failed: {0}. Authorization code: {1}", voidingProblem, this.Result);

                    // TODO: Notify the system integrity recovery component
                }

                this.Result = string.Empty;
            }
        }
    }
}