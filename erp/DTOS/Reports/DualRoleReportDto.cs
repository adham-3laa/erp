using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.Reports
{
    public class DualRoleReportDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("usernumber")]
        public int UserNumber { get; set; }

        [JsonPropertyName("fullname")]
        public string FullName { get; set; }

        [JsonPropertyName("phonenumber")]
        public string PhoneNumber { get; set; }

        // === Money FOR US (We are owed) ===
        [JsonPropertyName("totalmoneyforus")]
        public decimal TotalMoneyForUs { get; set; }

        [JsonPropertyName("pendingcustomersales")]
        public decimal PendingCustomerSales { get; set; }

        [JsonPropertyName("pendingsupplierreturns")]
        public decimal PendingSupplierReturns { get; set; }

        // === Money FOR HIM (We owe) ===
        [JsonPropertyName("totalmoneyforhim")]
        public decimal TotalMoneyForHim { get; set; }

        [JsonPropertyName("pendingsupplierpurchases")]
        public decimal PendingSupplierPurchases { get; set; }

        [JsonPropertyName("pendingcustomerreturns")]
        public decimal PendingCustomerReturns { get; set; }

        // === Net ===
        [JsonPropertyName("netbalance")]
        public decimal NetBalance { get; set; }

        [JsonPropertyName("balancestatus")]
        public string BalanceStatus { get; set; }

        [JsonPropertyName("customeractivity")]
        public DualRoleCustomerActivityDto CustomerActivity { get; set; }

        [JsonPropertyName("supplieractivity")]
        public DualRoleSupplierActivityDto SupplierActivity { get; set; }
    }

    public class DualRoleCustomerActivityDto
    {
        [JsonPropertyName("usernumber")]
        public int UserNumber { get; set; }

        [JsonPropertyName("customername")]
        public string CustomerName { get; set; }

        [JsonPropertyName("phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("totalorderscount")]
        public int TotalOrdersCount { get; set; }

        [JsonPropertyName("totalsalesamount")]
        public decimal TotalSalesAmount { get; set; }

        [JsonPropertyName("totalpaid")]
        public decimal TotalPaid { get; set; }

        [JsonPropertyName("totaldebt")]
        public decimal TotalDebt { get; set; }

        [JsonPropertyName("lastorderdate")]
        public DateTime? LastOrderDate { get; set; }

        [JsonPropertyName("totalreturnscount")]
        public int TotalReturnsCount { get; set; }

        [JsonPropertyName("totalreturnsamount")]
        public decimal TotalReturnsAmount { get; set; }

        [JsonPropertyName("totalremainingunpaidreturns")]
        public decimal TotalRemainingUnpaidReturns { get; set; }

        [JsonPropertyName("totalnetdebt")]
        public decimal TotalNetDebt { get; set; }

        [JsonPropertyName("unpaidinvoices")]
        public List<DualRoleUnpaidInvoiceDto> UnpaidInvoices { get; set; } = new();

        [JsonPropertyName("unpaidreturns")]
        public List<DualRoleUnpaidInvoiceDto> UnpaidReturns { get; set; } = new();
    }

    public class DualRoleSupplierActivityDto
    {
        [JsonPropertyName("usernumber")]
        public int UserNumber { get; set; }

        [JsonPropertyName("suppliername")]
        public string SupplierName { get; set; }

        [JsonPropertyName("phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("totalsupplycount")]
        public int TotalSupplyCount { get; set; }

        [JsonPropertyName("totalsupplyamount")]
        public decimal TotalSupplyAmount { get; set; }

        [JsonPropertyName("totalpaid")]
        public decimal TotalPaid { get; set; }

        [JsonPropertyName("totaldebt")]
        public decimal TotalDebt { get; set; }

        [JsonPropertyName("totalreturnscount")]
        public int TotalReturnsCount { get; set; }

        [JsonPropertyName("totalreturnsamount")]
        public decimal TotalReturnsAmount { get; set; }

        [JsonPropertyName("totalremainingunpaidreturns")]
        public decimal TotalRemainingUnpaidReturns { get; set; }

        [JsonPropertyName("totalnetdebt")]
        public decimal TotalNetDebt { get; set; }

        [JsonPropertyName("unpaidinvoices")]
        public List<DualRoleUnpaidInvoiceDto> UnpaidInvoices { get; set; } = new();

        [JsonPropertyName("unpaidreturns")]
        public List<DualRoleUnpaidInvoiceDto> UnpaidReturns { get; set; } = new();
    }

    public class DualRoleUnpaidInvoiceDto
    {
        [JsonPropertyName("invoicecode")]
        public int InvoiceCode { get; set; }

        [JsonPropertyName("originalamount")]
        public decimal OriginalAmount { get; set; }

        [JsonPropertyName("remainingamount")]
        public decimal RemainingAmount { get; set; }

        [JsonPropertyName("invoicedate")]
        public DateTime InvoiceDate { get; set; }
    }
}
