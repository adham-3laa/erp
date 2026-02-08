using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using erp.DTOS;
using erp.DTOS.InvoicesDTOS;
using System.Collections.Generic;
using System.Linq;
using erp.Enums;
using System;

namespace erp.Printing
{
    public class LedgerPdfDocument : IDocument
    {
        private readonly UserDto _user;
        private readonly IEnumerable<InvoiceResponseDto> _invoices;

        public LedgerPdfDocument(UserDto user, IEnumerable<InvoiceResponseDto> invoices)
        {
            _user = user;
            _invoices = invoices;
        }

        public DocumentMetadata GetMetadata()
            => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Determine User Label
                string userTypeLabel = "العميل";
                var uType = _user.UserType?.ToLower() ?? "";
                if (uType.Contains("supplier") || uType.Contains("مورد"))
                    userTypeLabel = "المورد";
                else if (uType.Contains("salesrep") || uType.Contains("مندوب"))
                    userTypeLabel = "مندوب المبيعات";

                // ================= HEADER =================
                page.Header().Column(headerCol =>
                {
                    // Title section with border
                    headerCol.Item()
                        .BorderBottom(2)
                        .BorderColor(Colors.Black)
                        .PaddingBottom(10)
                        .Row(row =>
                        {
                            // Left Side: Company Name
                            row.RelativeItem().AlignLeft().Column(col =>
                            {
                                col.Item().Text("The First").FontSize(20).Bold();
                            });

                            // Right Side: Ledger Info
                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text("كشف حساب")
                                    .FontSize(20)
                                    .Bold();

                                col.Item().PaddingTop(5).Text($"{userTypeLabel}: {_user.Fullname ?? "غير محدد"}")
                                    .FontSize(12);

                                col.Item().PaddingTop(3).Text($"التاريخ: {DateTime.Now:yyyy-MM-dd}")
                                    .FontSize(10);

                                col.Item().PaddingTop(3).Text($"عدد الفواتير: {_invoices.Count()}")
                                    .FontSize(10);
                            });
                        });

                    headerCol.Item().PaddingTop(15);
                });

                // ================= CONTENT =================
                page.Content().Column(content =>
                {
                    // Invoices Table
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.5f); // رقم الفاتورة
                            columns.RelativeColumn(2.5f); // نوع الفاتورة
                            columns.RelativeColumn(2f);   // التاريخ
                            columns.RelativeColumn(2f);   // الإجمالي
                            columns.RelativeColumn(2f);   // المدفوع
                            columns.RelativeColumn(2f);   // المتبقي
                        });

                        // ===== Header Row =====
                        table.Header(header =>
                        {
                            var headerStyle = TextStyle.Default.Bold().FontSize(10);

                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignCenter().Text("رقم").Style(headerStyle);
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignCenter().Text("نوع الفاتورة").Style(headerStyle);
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignCenter().Text("التاريخ").Style(headerStyle);
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignCenter().Text("الإجمالي").Style(headerStyle);
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignCenter().Text("المدفوع").Style(headerStyle);
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignCenter().Text("المتبقي").Style(headerStyle);
                        });

                        // ===== Data Rows =====
                        foreach (var inv in _invoices)
                        {
                            string invoiceTypeName = inv.InvoiceTypeParsed.GetArabicDisplayName();
                            bool isReturn = inv.InvoiceTypeParsed == InvoiceType.ReturnInvoice ||
                                           inv.InvoiceTypeParsed == InvoiceType.SupplierReturnInvoice;

                            // Custom override for better display
                            if (inv.InvoiceTypeParsed == InvoiceType.CustomerInvoice)
                                invoiceTypeName = "فاتورة مبيعات";
                            else if (inv.InvoiceTypeParsed == InvoiceType.ReturnInvoice)
                                invoiceTypeName = "مرتجع مبيعات";

                            // Mark returns with asterisk
                            if (isReturn)
                                invoiceTypeName = "* " + invoiceTypeName;

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Padding(5).AlignCenter().Text(inv.code.ToString());

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Padding(5).AlignCenter().Text(invoiceTypeName).FontSize(9);

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Padding(5).AlignCenter().Text(inv.GeneratedDate.ToString("yyyy-MM-dd"));

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Padding(5).AlignCenter().Text(inv.Amount.ToString("N2"));

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Padding(5).AlignCenter().Text(inv.PaidAmount.ToString("N2"));

                            table.Cell().Border(1).BorderColor(Colors.Black)
                                .Padding(5).AlignCenter().Text(inv.RemainingAmount.ToString("N2"));
                        }
                    });

                    // ================= SUMMARY SECTION =================
                    content.Item().PaddingTop(20);

                    // Calculate totals
                    var salesInvoices = _invoices.Where(i =>
                        i.InvoiceTypeParsed == InvoiceType.CustomerInvoice ||
                        i.InvoiceTypeParsed == InvoiceType.CommissionInvoice ||
                        i.InvoiceTypeParsed == InvoiceType.SupplierInvoice);

                    var returnInvoices = _invoices.Where(i =>
                        i.InvoiceTypeParsed == InvoiceType.ReturnInvoice ||
                        i.InvoiceTypeParsed == InvoiceType.SupplierReturnInvoice);

                    decimal totalSalesAmount = salesInvoices.Sum(x => x.Amount);
                    decimal totalSalesPaid = salesInvoices.Sum(x => x.PaidAmount);
                    decimal totalSalesRemaining = salesInvoices.Sum(x => x.RemainingAmount);

                    decimal totalReturnsAmount = returnInvoices.Sum(x => x.Amount);
                    decimal totalReturnsPaid = returnInvoices.Sum(x => x.PaidAmount);
                    decimal totalReturnsRemaining = returnInvoices.Sum(x => x.RemainingAmount);

                    // Determine user type
                    bool isSupplier = uType.Contains("supplier") || uType.Contains("مورد");
                    bool isSalesRep = uType.Contains("salesrep") || uType.Contains("مندوب");

                    // Labels based on user type
                    string invoicesLabel;
                    string invoicesDescription;
                    string returnsLabel;
                    string returnsDescription;
                    string netPositiveLabel;
                    string netNegativeLabel;
                    string noteText;

                    if (isSupplier)
                    {
                        // Supplier logic:
                        // - Supplier Invoice = Company owes supplier (لصالح المورد)
                        // - Supplier Return = Supplier owes company (على المورد)
                        invoicesLabel = "إجمالي فواتير المورد";
                        invoicesDescription = "(لصالح المورد - على الشركة)";
                        returnsLabel = "إجمالي مرتجعات المورد";
                        returnsDescription = "(على المورد - للشركة)";
                        netPositiveLabel = "صافي الرصيد لصالح المورد";
                        netNegativeLabel = "صافي الرصيد على المورد";
                        noteText = "* مرتجعات المورد تُخصم من إجمالي فواتير المورد";
                    }
                    else if (isSalesRep)
                    {
                        // Sales Rep logic:
                        // - Commission Invoice = Company owes sales rep (لصالح المندوب)
                        // - No returns for sales reps
                        invoicesLabel = "إجمالي العمولات";
                        invoicesDescription = "(لصالح المندوب - على الشركة)";
                        returnsLabel = "خصومات";
                        returnsDescription = "(على المندوب)";
                        netPositiveLabel = "صافي الرصيد لصالح المندوب";
                        netNegativeLabel = "صافي الرصيد على المندوب";
                        noteText = "";
                    }
                    else
                    {
                        // Customer logic:
                        // - Sales Invoice = Customer owes company (على العميل)
                        // - Return Invoice = Company owes customer (للعميل)
                        invoicesLabel = "إجمالي المبيعات";
                        invoicesDescription = $"(على {userTypeLabel})";
                        returnsLabel = "إجمالي المرتجعات";
                        returnsDescription = $"(لصالح {userTypeLabel})";
                        netPositiveLabel = $"صافي الرصيد على {userTypeLabel}";
                        netNegativeLabel = $"صافي الرصيد لصالح {userTypeLabel}";
                        noteText = "* المرتجعات تُخصم من إجمالي المبيعات";
                    }

                    // Net balance calculation
                    decimal netBalance = totalSalesRemaining - totalReturnsRemaining;

                    // Summary Table
                    content.Item().Table(summaryTable =>
                    {
                        summaryTable.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // Header
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                            .Padding(6).AlignCenter().Text("البيان").Bold();
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                            .Padding(6).AlignCenter().Text("الإجمالي").Bold();
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                            .Padding(6).AlignCenter().Text("المدفوع").Bold();
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                            .Padding(6).AlignCenter().Text("المتبقي").Bold();

                        // Invoices/Sales row
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black)
                            .Padding(5).AlignRight().Column(c =>
                            {
                                c.Item().Text(invoicesLabel).Bold();
                                c.Item().Text(invoicesDescription).FontSize(8);
                            });
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black)
                            .Padding(5).AlignCenter().Text(totalSalesAmount.ToString("N2"));
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black)
                            .Padding(5).AlignCenter().Text(totalSalesPaid.ToString("N2"));
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black)
                            .Padding(5).AlignCenter().Text(totalSalesRemaining.ToString("N2")).Bold();

                        // Returns row
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black)
                            .Padding(5).AlignRight().Column(c =>
                            {
                                c.Item().Text(returnsLabel).Bold();
                                c.Item().Text(returnsDescription).FontSize(8);
                            });
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black)
                            .Padding(5).AlignCenter().Text(totalReturnsAmount.ToString("N2"));
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black)
                            .Padding(5).AlignCenter().Text(totalReturnsPaid.ToString("N2"));
                        summaryTable.Cell().Border(1).BorderColor(Colors.Black)
                            .Padding(5).AlignCenter().Text(totalReturnsRemaining.ToString("N2")).Bold();
                    });

                    // Net Balance
                    content.Item().PaddingTop(15);
                    
                    string balanceDirection = netBalance >= 0 
                        ? netPositiveLabel 
                        : netNegativeLabel;

                    content.Item().Border(2).BorderColor(Colors.Black).Padding(12).Column(col =>
                    {
                        col.Item().AlignCenter().Text(balanceDirection).Bold().FontSize(12);
                        col.Item().PaddingTop(8).AlignCenter()
                            .Text($"{Math.Abs(netBalance):N2} ج.م")
                            .Bold().FontSize(18);
                    });

                    // Note for returns
                    if (returnInvoices.Any())
                    {
                        content.Item().PaddingTop(15).AlignRight()
                            .Text(noteText)
                            .FontSize(11).Bold();
                    }
                });

                // ================= FOOTER =================
                page.Footer()
                    .BorderTop(1)
                    .BorderColor(Colors.Black)
                    .PaddingTop(5)
                    .Row(row =>
                    {
                        

                        row.RelativeItem().AlignCenter().Text(text =>
                        {
                            text.Span("صفحة ");
                            text.CurrentPageNumber();
                            text.Span(" من ");
                            text.TotalPages();
                        });

                        row.RelativeItem().AlignLeft().Text($"{DateTime.Now:yyyy-MM-dd}")
                            .FontSize(8);
                    });
            });
        }
    }
}
