// -----------------------------------------------------------------------
// <copyright file="TransactionResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Holds summary information for a commerce transaction result.
    /// </summary>
    public class TransactionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionResult"/> class.
        /// </summary>
        /// <param name="amountCharged">The total amount charged for the transaction.</param>
        /// <param name="lineItems">A collection of line items bundled in the transaction.</param>
        /// <param name="timeStamp">The time at which the transaction took place.</param>
        public TransactionResult(decimal amountCharged, IEnumerable<TransactionResultLineItem> lineItems, DateTime timeStamp)
        {
            // we don't validate amount charged since a transaction may result in a negative amount
            if (lineItems == null || lineItems.Count() <= 0)
            {
                throw new ArgumentException("lineItems must at least have one line item", nameof(lineItems));
            }

            foreach (var lineItem in lineItems)
            {
                lineItem.AssertNotNull("lineItems has an empty entry");
            }

            this.AmountCharged = amountCharged;
            this.LineItems = lineItems;
            this.TimeStamp = timeStamp;
        }

        /// <summary>
        /// Gets the total amount charged for the transaction.
        /// </summary>
        public decimal AmountCharged { get; private set; }

        /// <summary>
        /// Gets the result line items associated with the transaction.
        /// </summary>
        public IEnumerable<TransactionResultLineItem> LineItems { get; private set; }

        /// <summary>
        /// Gets the time at which the transaction took place.
        /// </summary>
        public DateTime TimeStamp { get; private set; }
    }    
}