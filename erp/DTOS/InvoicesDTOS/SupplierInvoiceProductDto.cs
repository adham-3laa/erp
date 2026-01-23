using System.Text.Json.Serialization;

namespace erp.DTOS.InvoicesDTOS
{
    /// <summary>
    /// DTO for supplier invoice product items.
    /// Response from: GET /api/Invoices/GetSupplierInviceProductsByInvoicCode?invoiceCode={code}
    /// </summary>
    public class SupplierInvoiceProductDto
    {
        [JsonPropertyName("productid")]
        public string ProductId { get; set; } = "";

        [JsonPropertyName("productcode")]
        public int ProductCode { get; set; }

        [JsonPropertyName("productname")]
        public string ProductName { get; set; } = "";

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("buyprice")]
        public decimal BuyPrice { get; set; }

        [JsonPropertyName("totalprice")]
        public decimal TotalPrice { get; set; }
    }
}
