using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.DTOS.ExpensesDTOS
{
    public class ExpenseCreateDto
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
