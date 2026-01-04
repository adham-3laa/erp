using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using erp.DTOS;
using erp.DTOS.InvoicesDTOS;

namespace erp.Printing
{
    public class InvoicePdfDocument : IDocument
    {
        private readonly UserDto _user;
        private readonly InvoiceResponseDto _invoice;

        public InvoicePdfDocument(UserDto user, InvoiceResponseDto invoice)
        {
            _user = user;
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
                page.DefaultTextStyle(x => x.FontSize(11));

                // ================= HEADER =================
                page.Header().Row(row =>
                {
                    // الشركة (شمال)
                    row.RelativeItem().AlignLeft().Column(col =>
                    {
                        col.Item().Text("The First")
                                  .FontSize(18)
                                  .Bold();

                        col.Item().Text("Smart ERP System")
                                  .FontSize(9)
                                  .FontColor(Colors.Grey.Darken1);
                    });

                    // العنوان (يمين)
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("فاتورة")
                                  .FontSize(16)
                                  .Bold();

                        col.Item().Text($"رقم الفاتورة: {_invoice.Id}")
                                  .FontSize(9);
                    });
                });

                // ================= CONTENT =================
                page.Content().PaddingTop(20).Column(col =>
                {
                    // بيانات العميل
                    col.Item().Text($"العميل: {_user.Fullname}").Bold();
                    col.Item().Text($"البريد: {_user.Email}");
                    col.Item().Text($"تاريخ الفاتورة: {_invoice.GeneratedDate:yyyy-MM-dd}");

                    col.Item().PaddingVertical(10);
                    col.Item().LineHorizontal(1);
                    col.Item().PaddingVertical(10);

                    // جدول الفاتورة
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3)
                                         .Padding(6)
                                         .Text("البيان")
                                         .Bold();

                            header.Cell().Background(Colors.Grey.Lighten3)
                                         .Padding(6)
                                         .Text("القيمة")
                                         .Bold();
                        });

                        // Rows
                        table.Cell().Padding(6).Text("الإجمالي");
                        table.Cell().Padding(6).Text(_invoice.Amount.ToString("N2"));

                        table.Cell().Padding(6).Text("المدفوع");
                        table.Cell().Padding(6).Text(_invoice.PaidAmount.ToString("N2"));

                        table.Cell().Padding(6).Text("المتبقي");
                        table.Cell().Padding(6).Text(_invoice.RemainingAmount.ToString("N2"));
                    });
                });

                // ================= FOOTER =================
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("The First ERP  |  ");
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
