using System;
using System.Collections.Generic;
using System.Linq;

namespace erp.DTOS.InvoicesDTOS
{
    public class PrintableInvoiceDto
    {
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Title to display on the invoice (e.g. فاتورة مبيعات, فاتورة مرتجع)
        /// </summary>
        public string InvoiceTypeTitle { get; set; } = "فاتورة";
        
        /// <summary>
        /// Invoice type for special handling (e.g. Commission invoices)
        /// </summary>
        public erp.Enums.InvoiceType? InvoiceType { get; set; }
        
        /// <summary>
        /// Sequential invoice code for display in printed documents.
        /// This is the human-readable invoice number.
        /// </summary>
        public int InvoiceCode { get; set; }
        
        public DateTime InvoiceDate { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }

        // Order
        public string OrderId { get; set; }

        public List<PrintableInvoiceItemDto> Items { get; set; } = new();

        // Totals
        public decimal SubTotal => Items.Sum(x => x.Total);
        
        /// <summary>
        /// For commission invoices, this is the actual commission amount (not the total sales)
        /// </summary>
        public decimal? CommissionAmount { get; set; }
        
        /// <summary>
        /// Returns CommissionAmount for commission invoices, otherwise SubTotal
        /// </summary>
        public decimal DisplayTotal => CommissionAmount ?? SubTotal;
        
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    public class PrintableInvoiceItemDto
    {
        public string ProductName { get; set; }
        public string CategoryName { get; set; } = "-";
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }

        public decimal Total => UnitPrice * Quantity;
    }
}
