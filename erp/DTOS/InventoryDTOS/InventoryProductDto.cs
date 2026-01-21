using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.DTOS.InventoryDTOS
{
    public class InventoryProductDto
    {
        public string productid { get; set; } = "";
        public int code { get; set; }
        public string productname { get; set; } = "";
        public decimal saleprice { get; set; }
        public decimal buyprice { get; set; }
        public int quantity { get; set; }
        public string sku { get; set; } = "";
        public string description { get; set; } = "";
        public string? categoryname { get; set; }
        public string? categoryid { get; set; }
    }
}
