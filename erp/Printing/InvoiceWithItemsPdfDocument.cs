using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using erp.DTOS.InvoicesDTOS;

namespace erp.Printing
{
    public class InvoiceWithItemsPdfDocument : IDocument
    {
        private readonly PrintableInvoiceDto _invoice;

        public InvoiceWithItemsPdfDocument(PrintableInvoiceDto invoice)
        {
            _invoice = invoice;
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

                // ===== Header =====
                page.Header().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Column(col =>
                    {
                        col.Item().Text("The First").FontSize(18).Bold();
                        col.Item().Text("Smart ERP System")
                                  .FontSize(9)
                                  .FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("رقم الفاتورة").FontSize(16).Bold();
                        col.Item().Text($"{_invoice.InvoiceId}")
                                  .FontSize(9);
                    });
                });

                // ===== Content =====
                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().Text($"العميل: {_invoice.CustomerName}").Bold();
                    col.Item().Text($"البريد: {_invoice.CustomerEmail}");
                    col.Item().Text($"تاريخ الفاتورة: {_invoice.InvoiceDate:yyyy-MM-dd}");
                    col.Item().PaddingVertical(10);

                    // ===== Items Table =====
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3); // المنتج
                            c.RelativeColumn(2); // الفئة
                            c.RelativeColumn(2); // السعر
                            c.RelativeColumn(1); // الكمية
                            c.RelativeColumn(2); // الإجمالي
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("المنتج").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("الفئة").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("السعر").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("الكمية").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("الإجمالي").Bold();
                        });

                        foreach (var item in _invoice.Items)
                        {
                            table.Cell().Padding(5).Text(item.ProductName);
                            table.Cell().Padding(5).Text(item.CategoryName);
                            table.Cell().Padding(5).Text(item.UnitPrice.ToString("N2"));
                            table.Cell().Padding(5).Text(item.Quantity.ToString());
                            table.Cell().Padding(5).Text(item.Total.ToString("N2"));
                        }
                    });

                    col.Item().PaddingVertical(10);

                    // ===== Totals =====
                    col.Item().AlignRight().Text($"الإجمالي: {_invoice.SubTotal:N2}").Bold();
                    col.Item().AlignRight().Text($"المدفوع: {_invoice.PaidAmount:N2}");
                    col.Item().AlignRight().Text($"المتبقي: {_invoice.RemainingAmount:N2}").Bold();
                });

                // ===== Footer =====
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("The First ERP | ");
                    t.Span($"طباعة: {DateTime.Now:yyyy-MM-dd} | ");
                    t.Span("صفحة ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        }
    }
}
