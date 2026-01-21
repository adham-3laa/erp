using System.Text.Json.Serialization;

namespace erp.DTOS
{
    /// <summary>
    /// DTO for products returned from supplier invoice endpoint
    /// </summary>
    public class SupplierInvoiceProductDto
    {
        [JsonPropertyName("productid")]
        public string ProductId { get; set; }

        [JsonPropertyName("productcode")]
        public int ProductCode { get; set; }

        [JsonPropertyName("productname")]
        public string ProductName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("buyprice")]
        public decimal BuyPrice { get; set; }

        [JsonPropertyName("totalprice")]
        public decimal TotalPrice { get; set; }
    }
}
