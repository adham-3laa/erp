using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.Reports
{
    public class CustomerReportDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("usernumber")]
        public int UserNumber { get; set; }

        [JsonPropertyName("customername")]
        public string CustomerName { get; set; }

        [JsonPropertyName("phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("totalorderscount")]
        public int TotalOrdersCount { get; set; }

        [JsonPropertyName("totalsalesamount")]
        public decimal TotalSalesAmount { get; set; }

        [JsonPropertyName("totalpaid")]
        public decimal TotalPaid { get; set; }

        [JsonPropertyName("totaldebt")]
        public decimal TotalDebt { get; set; }

        [JsonPropertyName("lastorderdate")]
        public DateTime? LastOrderDate { get; set; }

        [JsonPropertyName("totalreturnscount")]
        public int TotalReturnsCount { get; set; }

        [JsonPropertyName("totalreturnsamount")]
        public decimal TotalReturnsAmount { get; set; }

        [JsonPropertyName("totalnetdebt")]
        public decimal TotalNetDebt { get; set; }

        [JsonPropertyName("unpaidinvoices")]
        public List<UnpaidInvoiceDto> UnpaidInvoices { get; set; } = new();

        [JsonPropertyName("unpaidreturns")]
        public List<UnpaidInvoiceDto> UnpaidReturns { get; set; } = new();
    }

    public class UnpaidInvoiceDto
    {
        [JsonPropertyName("invoicecode")]
        public int InvoiceCode { get; set; }

        [JsonPropertyName("originalamount")]
        public decimal OriginalAmount { get; set; }

        [JsonPropertyName("remainingamount")]
        public decimal RemainingAmount { get; set; }

        [JsonPropertyName("invoicedate")]
        public DateTime InvoiceDate { get; set; }
    }
}
