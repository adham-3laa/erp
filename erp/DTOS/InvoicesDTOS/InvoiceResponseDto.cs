using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.DTOS.InvoicesDTOS
{
    public class InvoiceResponseDto
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public string? RecipientName { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime GeneratedDate { get; set; }
        public Guid? OrderId { get; set; }
    }
}
