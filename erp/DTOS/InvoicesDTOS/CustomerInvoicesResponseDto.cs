using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.InvoicesDTOS
{
    /// <summary>
    /// Response DTO for customer invoices endpoint:
    /// GET /api/Invoices/AllInvoicesForSpecificCustomerByCustomerId
    /// 
    /// Contains sales statistics, returns statistics, and list of all invoices.
    /// </summary>
    public class CustomerInvoicesResponseDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = "";

        // ==================== Sales Statistics ====================
        
        /// <summary>
        /// Number of sales invoices (CustomerInvoice)
        /// </summary>
        [JsonPropertyName("salescount")]
        public int SalesCount { get; set; }

        /// <summary>
        /// Total amount of all sales invoices
        /// </summary>
        [JsonPropertyName("salestotalamount")]
        public decimal SalesTotalAmount { get; set; }

        /// <summary>
        /// Total paid amount across all sales invoices
        /// </summary>
        [JsonPropertyName("salespaidamount")]
        public decimal SalesPaidAmount { get; set; }

        /// <summary>
        /// Total remaining amount across all sales invoices
        /// </summary>
        [JsonPropertyName("salesremainingamount")]
        public decimal SalesRemainingAmount { get; set; }

        // ==================== Returns Statistics ====================
        
        /// <summary>
        /// Number of return invoices
        /// </summary>
        [JsonPropertyName("returnscount")]
        public int ReturnsCount { get; set; }

        /// <summary>
        /// Total amount of all return invoices
        /// </summary>
        [JsonPropertyName("returnstotalamount")]
        public decimal ReturnsTotalAmount { get; set; }

        /// <summary>
        /// Total paid amount across all return invoices
        /// </summary>
        [JsonPropertyName("returnspaidamount")]
        public decimal ReturnsPaidAmount { get; set; }

        /// <summary>
        /// Total remaining amount across all return invoices
        /// </summary>
        [JsonPropertyName("returnsremainingamount")]
        public decimal ReturnsRemainingAmount { get; set; }

        // ==================== Invoices List ====================
        
        /// <summary>
        /// List of all invoices (both sales and returns)
        /// </summary>
        [JsonPropertyName("invoices")]
        public List<InvoiceResponseDto> Invoices { get; set; } = new();

        // ==================== Computed Properties ====================
        
        /// <summary>
        /// Total count of all invoices (sales + returns)
        /// </summary>
        [JsonIgnore]
        public int TotalInvoicesCount => SalesCount + ReturnsCount;

        /// <summary>
        /// Total amount (sales + returns)
        /// </summary>
        [JsonIgnore]
        public decimal TotalAmount => SalesTotalAmount + ReturnsTotalAmount;

        /// <summary>
        /// Total paid (sales + returns)
        /// </summary>
        [JsonIgnore]
        public decimal TotalPaid => SalesPaidAmount + ReturnsPaidAmount;

        /// <summary>
        /// Total remaining (sales + returns)
        /// </summary>
        [JsonIgnore]
        public decimal TotalRemaining => SalesRemainingAmount + ReturnsRemainingAmount;
    }
}
