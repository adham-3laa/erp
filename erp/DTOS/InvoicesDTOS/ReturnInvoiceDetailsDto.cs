using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.InvoicesDTOS
{
    public class ReturnInvoiceDetailsDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("invoiceid")]
        public string InvoiceId { get; set; }

        [JsonPropertyName("invoicecode")]
        public string InvoiceCode { get; set; }

        [JsonPropertyName("refundamount")]
        public decimal RefundAmount { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("items")]
        public List<ReturnInvoiceItemDto> Items { get; set; } = new List<ReturnInvoiceItemDto>();
    }

    public class ReturnInvoiceItemDto
    {
        [JsonPropertyName("productname")]
        public string ProductName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitprice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("totalprice")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
}
