using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using erp.DTOS;
using erp.DTOS.InvoicesDTOS;
using System.Collections.Generic;
using System.Linq;
using erp.Enums;

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
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Determine User Label
                string userTypeLabel = "العميل";
                var uType = _user.UserType?.ToLower() ?? "";
                if (uType.Contains("supplier") || uType.Contains("مورد")) userTypeLabel = "المورد";
                else if (uType.Contains("salesrep") || uType.Contains("مندوب")) userTypeLabel = "مندوب المبيعات";

                // ================= HEADER =================
                page.Header().Row(row =>
                {
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("كشف حساب")
                                  .FontSize(16)
                                  .Bold();

                        col.Item().Text($"{_user.Fullname} : {userTypeLabel}")
                                  .FontSize(10);
                    });
                });

                // ================= CONTENT =================
                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // رقم الفاتورة
                        columns.RelativeColumn(3); // نوع الفاتورة (NEW)
                        columns.RelativeColumn(2); // التاريخ
                        columns.RelativeColumn(2); // الإجمالي
                        columns.RelativeColumn(2); // المدفوع
                        columns.RelativeColumn(2); // المتبقي
                    });

                    // ===== Header Row =====
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("رقم الفاتورة").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("نوع الفاتورة").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("التاريخ").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("الإجمالي").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("المدفوع").Bold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("المتبقي").Bold();
                    });


                    // ===== Data Rows =====
                    foreach (var inv in _invoices)
                    {
                        // Helper logic for Arabic Invoice Type using existing extension
                        string invoiceTypeName = inv.InvoiceTypeParsed.GetArabicDisplayName();
                        
                        // Custom override if specifically wanted "فاتورة مبيعات" instead of "فاتورة عميل"
                        if (inv.InvoiceTypeParsed == InvoiceType.CustomerInvoice)
                            invoiceTypeName = "فاتورة مبيعات";
                        else if (inv.InvoiceTypeParsed == InvoiceType.ReturnInvoice)
                            invoiceTypeName = "مرتجع مبيعات";

                        table.Cell().Padding(5).Text(inv.code.ToString());
                        
                        // Badge style for type
                        var typeContainer = table.Cell().Padding(5);
                        if (inv.InvoiceTypeParsed == InvoiceType.ReturnInvoice || 
                            inv.InvoiceTypeParsed == InvoiceType.SupplierReturnInvoice)
                        {
                            typeContainer.Text(invoiceTypeName).FontSize(10);
                        }
                        else
                        {
                            typeContainer.Text(invoiceTypeName);
                        }

                        table.Cell().Padding(5).Text(inv.GeneratedDate.ToString("yyyy-MM-dd"));
                        table.Cell().Padding(5).Text(inv.Amount.ToString("N2"));
                        table.Cell().Padding(5).Text(inv.PaidAmount.ToString("N2"));
                        table.Cell().Padding(5).Text(inv.RemainingAmount.ToString("N2"));
                    }

                    // ===== Total Row =====
                    table.Cell().ColumnSpan(3).PaddingTop(8).Text("الإجمالي الكلي").Bold();

                    table.Cell().PaddingTop(8)
                        .Text(_invoices.Sum(x => x.Amount).ToString("N2"))
                        .Bold();

                    table.Cell().PaddingTop(8)
                        .Text(_invoices.Sum(x => x.PaidAmount).ToString("N2"))
                        .Bold();

                    table.Cell().PaddingTop(8)
                        .Text(_invoices.Sum(x => x.RemainingAmount).ToString("N2"))
                        .Bold();
                });

                // ================= FOOTER =================
                page.Footer().AlignCenter().Text(text =>
                {
                    //text.Span("The First ERP  |  ");
                    text.Span($"تاريخ الطباعة: {DateTime.Now:yyyy-MM-dd}  |  ");
                    text.Span("صفحة ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }
    }
}
