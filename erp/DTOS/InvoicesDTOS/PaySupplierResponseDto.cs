using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.DTOS.InvoicesDTOS
{
    public class PaySupplierResponseDto
    {
        public decimal Amount { get; set; }          // إجمالي قيمة الفاتورة
        public decimal PaidAmount { get; set; }      // إجمالي المدفوع
        public decimal RemainingAmount { get; set; } // المتبقي
        public string Supplier { get; set; }         // اسم المورد
    }
}
