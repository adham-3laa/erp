using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.InvoicesDTOS
{
    /// <summary>
    /// Response DTO for supplier invoices endpoint:
    /// GET /api/Invoices/AllInvoicesForSpecificSupplierBySupplierId
    /// 
    /// Contains supply statistics, returns statistics, and list of all invoices.
    /// </summary>
    public class SupplierInvoicesResponseDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = "";

        // ==================== Supply Statistics ====================
        
        /// <summary>
        /// Number of supply invoices (SupplierInvoice)
        /// </summary>
        [JsonPropertyName("supplycount")]
        public int SupplyCount { get; set; }

        /// <summary>
        /// Total amount of all supply invoices
        /// </summary>
        [JsonPropertyName("supplytotalamount")]
        public decimal SupplyTotalAmount { get; set; }

        /// <summary>
        /// Total paid amount across all supply invoices
        /// </summary>
        [JsonPropertyName("supplypaidamount")]
        public decimal SupplyPaidAmount { get; set; }

        /// <summary>
        /// Total remaining amount across all supply invoices
        /// </summary>
        [JsonPropertyName("supplyremainingamount")]
        public decimal SupplyRemainingAmount { get; set; }

        // ==================== Returns Statistics ====================
        
        /// <summary>
        /// Number of supplier return invoices
        /// </summary>
        [JsonPropertyName("returnscount")]
        public int ReturnsCount { get; set; }

        /// <summary>
        /// Total amount of all supplier return invoices
        /// </summary>
        [JsonPropertyName("returnstotalamount")]
        public decimal ReturnsTotalAmount { get; set; }

        /// <summary>
        /// Total paid amount across all supplier return invoices
        /// </summary>
        [JsonPropertyName("returnspaidamount")]
        public decimal ReturnsPaidAmount { get; set; }

        /// <summary>
        /// Total remaining amount across all supplier return invoices
        /// </summary>
        [JsonPropertyName("returnsremainingamount")]
        public decimal ReturnsRemainingAmount { get; set; }

        // ==================== Invoices List ====================
        
        /// <summary>
        /// List of all invoices (both supply and returns)
        /// </summary>
        [JsonPropertyName("invoices")]
        public List<InvoiceResponseDto> Invoices { get; set; } = new();

        // ==================== Computed Properties ====================
        
        /// <summary>
        /// Total count of all invoices (supply + returns)
        /// </summary>
        [JsonIgnore]
        public int TotalInvoicesCount => SupplyCount + ReturnsCount;

        /// <summary>
        /// Total amount (supply + returns)
        /// </summary>
        [JsonIgnore]
        public decimal TotalAmount => SupplyTotalAmount + ReturnsTotalAmount;

        /// <summary>
        /// Total paid (supply + returns)
        /// </summary>
        [JsonIgnore]
        public decimal TotalPaid => SupplyPaidAmount + ReturnsPaidAmount;

        /// <summary>
        /// Total remaining (supply + returns)
        /// </summary>
        [JsonIgnore]
        public decimal TotalRemaining => SupplyRemainingAmount + ReturnsRemainingAmount;
    }
}
