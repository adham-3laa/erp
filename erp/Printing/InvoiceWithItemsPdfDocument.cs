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
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial")); // Default font

                // Black and White Theme
                var titleColor = Colors.Black;
                var headerBgColor = Colors.Grey.Lighten3;
                var borderColor = Colors.Grey.Lighten2;

                // ===== Header =====
                page.Header().Row(row =>
                {
                    // Logo / Company Name
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("The First").FontSize(20).Bold().FontColor(titleColor);
                        col.Item().Text("Smart ERP System")
                                  .FontSize(10)
                                  .FontColor(Colors.Grey.Darken2);
                    });

                    // Invoice Details (Right Aligned)
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("فاتورة ضريبية").FontSize(18).Bold().FontColor(titleColor);
                        
                        col.Item().PaddingTop(25).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.ConstantColumn(10);
                                c.RelativeColumn();
                            });

                            table.Cell().AlignRight().Text($"{_invoice.CustomerName}").FontSize(14).Bold();
                            table.Cell(); // spacer
                            table.Cell().AlignRight().Text(": اسم العميل").FontSize(14).SemiBold();

                            table.Cell().AlignRight().Text($"{_invoice.InvoiceCode}").FontSize(14).Bold();
                            table.Cell(); // spacer
                            table.Cell().AlignRight().Text(": رقم الفاتورة").FontSize(14).SemiBold();
                            
                            if (!string.IsNullOrEmpty(_invoice.OrderId))
                            {
                                table.Cell().AlignRight().Text($"{_invoice.OrderId}").FontSize(14).Bold();
                                table.Cell();
                                table.Cell().AlignRight().Text(": رقم الطلب").FontSize(14).SemiBold();
                            }

                            table.Cell().AlignRight().Text($"{_invoice.InvoiceDate:yyyy-MM-dd}").FontSize(14);
                            table.Cell();
                            table.Cell().AlignRight().Text(": التاريخ").SemiBold().FontSize(14);
                        });
                    });
                });

                // ===== Content =====
                page.Content().PaddingTop(25).Column(col =>
                {
                    //// Customer Section
                    //col.Item().BorderBottom(1).BorderColor(borderColor).PaddingBottom(5).Row(row => 
                    //{
                    //    row.RelativeItem().Column(c =>
                    //    {
                    //        c.Item().Text("بيانات العميل").FontSize(12).Bold().FontColor(titleColor);
                    //        c.Item().Text($"{_invoice.CustomerName}").FontSize(11).Bold();
                    //        if (!string.IsNullOrEmpty(_invoice.CustomerEmail))
                    //            c.Item().Text($"{_invoice.CustomerEmail}").FontSize(9).FontColor(Colors.Grey.Darken2);
                    //    });
                    //});
                    
                    col.Item().PaddingVertical(15);

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

                        // Table Header
                        table.Header(h =>
                        {
                            h.Cell().Background(headerBgColor).Padding(5).Text("المنتج").Bold();
                            h.Cell().Background(headerBgColor).Padding(5).Text("الفئة").Bold();
                            h.Cell().Background(headerBgColor).Padding(5).Text("السعر").Bold();
                            h.Cell().Background(headerBgColor).Padding(5).Text("الكمية").Bold();
                            h.Cell().Background(headerBgColor).Padding(5).Text("الإجمالي").Bold();
                        });

                        // Table Rows
                        for (int i = 0; i < _invoice.Items.Count; i++)
                        {
                            var item = _invoice.Items[i];
                            // No alternating colors for pure B&W/Simple look, or maybe just borders
                            // Let's use simple borders as often requested in B&W
                             
                            table.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text(item.ProductName);
                            table.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text(item.CategoryName);
                            table.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text(item.UnitPrice.ToString("N2"));
                            table.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text(item.Quantity.ToString());
                            table.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text(item.Total.ToString("N2"));
                        }
                    });

                    col.Item().PaddingVertical(15);

                    // ===== Totals =====
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(); // Spacer
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Table(t =>
                            {
                                t.ColumnsDefinition(cols => 
                                {
                                    cols.RelativeColumn();
                                    cols.RelativeColumn();
                                });

                                t.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text("الإجمالي").SemiBold();
                                t.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).AlignLeft().Text($"{_invoice.SubTotal:N2}").Bold();

                                t.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).Text("المدفوع");
                                t.Cell().BorderBottom(1).BorderColor(borderColor).Padding(5).AlignLeft().Text($"{_invoice.PaidAmount:N2}");

                                t.Cell().Padding(5).Text("المتبقي").Bold();
                                t.Cell().Padding(5).AlignLeft().Text($"{_invoice.RemainingAmount:N2}").Bold();
                            });
                        });
                    });
                });

                // ===== Footer =====
                page.Footer().PaddingTop(10).AlignCenter().Column(col => 
                {
                    col.Item().ShowOnce().LineHorizontal(1).LineColor(borderColor);
                    col.Item().PaddingTop(5).Row(row => 
                    {
                        row.RelativeItem().AlignLeft().Text(t => 
                        {
                             t.Span("The First ERP | ").FontSize(9).FontColor(Colors.Grey.Darken2);
                             t.Span(DateTime.Now.ToString("yyyy-MM-dd")).FontSize(9);
                        });
                        
                        row.RelativeItem().AlignRight().Text(t =>
                        {
                            t.Span("صفحة ");
                            t.CurrentPageNumber();
                            t.Span(" من ");
                            t.TotalPages();
                        });
                    });
                });
            });
        }
    }
}
