using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace erp.DTOS.OrderDTOs
{
    public class OrderItemDto
    {
        // خليه بدون JsonPropertyName عشان يقبل productId / productid / ProductId ... إلخ
        public string ProductId { get; set; } = "";

        public string ProductName { get; set; } = "";

        public decimal Quantity { get; set; }

        // ساعات الـ API بيرجع Price = 0
        public decimal Price { get; set; }

        public decimal Total => Quantity * Price;
    }
}

