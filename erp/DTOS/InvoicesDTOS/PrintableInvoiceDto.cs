using System;
using System.Collections.Generic;
using System.Linq;

namespace erp.DTOS.InvoicesDTOS
{
    public class PrintableInvoiceDto
    {
        public Guid InvoiceId { get; set; }
        
        /// <summary>
        /// Sequential invoice code for display in printed documents.
        /// This is the human-readable invoice number.
        /// </summary>
        public int InvoiceCode { get; set; }
        
        public DateTime InvoiceDate { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        
        /// <summary>
        /// Display Title (Client Invoice, Supplier Invoice, etc.)
        /// </summary>
        public string InvoiceTitle { get; set; } = "فاتورة ضريبية";

        // Order
        public string OrderId { get; set; }

        public List<PrintableInvoiceItemDto> Items { get; set; } = new();

        // Totals
        public decimal SubTotal => Items.Sum(x => x.Total);
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
