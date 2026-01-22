using System.Text.Json.Serialization;

namespace erp.DTOS.InvoicesDTOS
{
    /// <summary>
    /// Response DTO for Supplier Invoice/Return payments.
    /// 
    /// Used by endpoint:
    /// PUT /api/Invoices/PayPartOfMoneyToSupplierBySupplierInvoiceId
    /// 
    /// Response example:
    /// {
    ///   "statusCode": 200,
    ///   "message": "Success",
    ///   "traceId": "...",
    ///   "amount": 36000,
    ///   "paidamount": 2000,
    ///   "remainingamount": 34000,
    ///   "supplier": "طه انور طه"
    /// }
    /// </summary>
    public class SupplierPaymentResponseDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("paidamount")]
        public decimal PaidAmount { get; set; }

        [JsonPropertyName("remainingamount")]
        public decimal RemainingAmount { get; set; }

        [JsonPropertyName("supplier")]
        public string? Supplier { get; set; }

        /// <summary>
        /// Indicates if the payment was successful.
        /// </summary>
        public bool IsSuccess => StatusCode == 200;
    }
}
