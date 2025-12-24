using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.DTOS.InvoicesDTOS
{
    public class PaidAmountResponseDto
    {
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        // هييجي واحد بس حسب النوع
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        public string SalesRepName { get; set; }
    }
}
