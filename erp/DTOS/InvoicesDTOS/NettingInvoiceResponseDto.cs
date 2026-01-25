using System;
using System.Collections.Generic;

namespace erp.DTOS.InvoicesDTOS
{
    /// <summary>
    /// Response DTO for the Netting Invoice (Sales & Return Invoice) endpoint.
    /// Endpoint: GET /api/Invoices/netting-invoice
    /// 
    /// This endpoint returns a comprehensive financial summary comparing
    /// sales invoices vs supply (return) invoices for a specific partner.
    /// </summary>
    public class NettingInvoiceResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Print date/time stamp
        /// </summary>
        public DateTime PrintDate { get; set; }
        
        /// <summary>
        /// Customer/Partner name (Arabic)
        /// </summary>
        public string PartnerName { get; set; } = string.Empty;
        
        /// <summary>
        /// Customer phone number (may be empty)
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Report start date
        /// </summary>
        public DateTime FromDate { get; set; }
        
        /// <summary>
        /// Report end date
        /// </summary>
        public DateTime ToDate { get; set; }
        
        /// <summary>
        /// List of sales invoice items
        /// </summary>
        public List<NettingInvoiceItemDto> SalesItems { get; set; } = new();
        
        /// <summary>
        /// Total amount of all sales invoices
        /// </summary>
        public decimal TotalSalesAmount { get; set; }
        
        /// <summary>
        /// List of supply/return invoice items
        /// </summary>
        public List<NettingInvoiceItemDto> SupplyItems { get; set; } = new();
        
        /// <summary>
        /// Total amount of all supply invoices
        /// </summary>
        public decimal TotalSupplyAmount { get; set; }
        
        /// <summary>
        /// Net balance (Sales - Supply)
        /// Negative = Customer has credit (له مستحقات)
        /// Positive = Customer owes money (عليه مستحقات)
        /// </summary>
        public decimal NetBalance { get; set; }
        
        /// <summary>
        /// Human-readable description of the balance status (Arabic)
        /// </summary>
        public string BalanceDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual invoice item within the netting report
    /// </summary>
    public class NettingInvoiceItemDto
    {
        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Invoice reference code (sequential invoice number)
        /// </summary>
        public string ReferenceCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Invoice type description (Arabic, e.g., "فاتورة بيع", "فاتورة توريد")
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Invoice total amount
        /// </summary>
        public decimal Amount { get; set; }
    }
}
