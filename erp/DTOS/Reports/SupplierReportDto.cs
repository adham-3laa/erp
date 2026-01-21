using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.Reports
{
    public class SupplierReportDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("usernumber")]
        public int UserNumber { get; set; }

        [JsonPropertyName("suppliername")]
        public string SupplierName { get; set; }

        [JsonPropertyName("phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("totalsupplycount")]
        public int TotalSupplyCount { get; set; }

        [JsonPropertyName("totalsupplyamount")]
        public decimal TotalSupplyAmount { get; set; }

        [JsonPropertyName("totalpaid")]
        public decimal TotalPaid { get; set; }

        [JsonPropertyName("totaldebt")]
        public decimal TotalDebt { get; set; }

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
}
