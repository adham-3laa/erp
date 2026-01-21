using System;

namespace erp.DTOS.Cheques
{
    public class UpdateChequeRequest
    {
        public int Code { get; set; }
        public string CheckNumber { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string BankName { get; set; } = "";
        public bool IsIncoming { get; set; }
        public string RelatedName { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}
