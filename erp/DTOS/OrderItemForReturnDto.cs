using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class OrderItemForReturnDto
    {
        [JsonPropertyName("productid")]
        public string Productid { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        
        [JsonPropertyName("unitprice")]
        public decimal Unitprice { get; set; }
        
        [JsonPropertyName("customerid")]
        public string Customerid { get; set; }
        
        [JsonPropertyName("productname")]
        public string Productname { get; set; }
    }
}
