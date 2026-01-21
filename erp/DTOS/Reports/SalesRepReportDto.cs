using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.Reports
{
    public class SalesRepReportDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("usernumber")]
        public int UserNumber { get; set; }

        [JsonPropertyName("salesrepname")]
        public string SalesRepName { get; set; }

        [JsonPropertyName("phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("totalorderscount")]
        public int TotalOrdersCount { get; set; }

        [JsonPropertyName("totalsalesvolume")]
        public decimal TotalSalesVolume { get; set; }

        [JsonPropertyName("totalreturnscount")]
        public int TotalReturnsCount { get; set; }

        [JsonPropertyName("totalreturnsvolume")]
        public decimal TotalReturnsVolume { get; set; }

        [JsonPropertyName("totalcommissionearned")]
        public decimal TotalCommissionEarned { get; set; }

        [JsonPropertyName("totalcommissionpaid")]
        public decimal TotalCommissionPaid { get; set; }

        [JsonPropertyName("totalcommissiondue")]
        public decimal TotalCommissionDue { get; set; }

        [JsonPropertyName("unpaidcommissions")]
        public List<UnpaidCommissionDto> UnpaidCommissions { get; set; } = new();
    }

    public class UnpaidCommissionDto
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
