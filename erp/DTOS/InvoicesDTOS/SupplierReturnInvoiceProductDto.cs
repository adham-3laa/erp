using System.Text.Json.Serialization;

namespace erp.DTOS.InvoicesDTOS
{
    /// <summary>
    /// DTO for Supplier Return Invoice product items.
    /// 
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// MANDATORY ENDPOINT (Single Source of Truth):
    /// GET /api/Returns/GetAllProductsInSpecificReturnSupplierInvoice?invoiceCode={code}
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// 
    /// This DTO maps the API response for products returned to a supplier.
    /// Each item represents:
    /// - Product returned to supplier
    /// - Quantity returned
    /// - Buy price at time of return
    /// - Total price (quantity × buyPrice)
    /// 
    /// ⚠️ CONSTRAINTS:
    /// - Use invoiceCode ONLY (do NOT use OrderId, CustomerId, or SalesRepId)
    /// - This is an ERP-critical financial document
    /// - Affects both inventory and accounting
    /// </summary>
    public class SupplierReturnInvoiceProductDto
    {
        /// <summary>
        /// Unique product identifier (GUID string)
        /// </summary>
        [JsonPropertyName("productid")]
        public string ProductId { get; set; } = "";

        /// <summary>
        /// Sequential product code (human-readable)
        /// </summary>
        [JsonPropertyName("productcode")]
        public int ProductCode { get; set; }

        /// <summary>
        /// Product display name
        /// </summary>
        [JsonPropertyName("productname")]
        public string ProductName { get; set; } = "";

        /// <summary>
        /// Quantity returned to supplier
        /// </summary>
        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Buy price per unit at time of return
        /// </summary>
        [JsonPropertyName("buyprice")]
        public decimal BuyPrice { get; set; }

        /// <summary>
        /// Total price = Quantity × BuyPrice
        /// </summary>
        [JsonPropertyName("totalprice")]
        public decimal TotalPrice { get; set; }
    }
}
