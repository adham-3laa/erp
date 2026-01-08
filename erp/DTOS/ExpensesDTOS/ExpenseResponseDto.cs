using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.DTOS
{
    public class ExpenseResponseDto
    {
        public string Id { get; set; }
        public int code { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AccountantUserId { get; set; }
        public string AccountantName { get; set; }
    }
}
