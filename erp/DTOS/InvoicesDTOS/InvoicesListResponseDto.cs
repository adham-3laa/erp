using System.Collections.Generic;

namespace erp.DTOS.InvoicesDTOS
{
    public class InvoicesListResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string TraceId { get; set; }

        public List<InvoiceResponseDto> Items { get; set; }

        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
