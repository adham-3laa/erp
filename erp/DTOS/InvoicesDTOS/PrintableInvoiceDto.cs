using System;
using System.Collections.Generic;
using System.Linq;

namespace erp.DTOS.InvoicesDTOS
{
    public class PrintableInvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }

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
