using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using erp.DTOS.InvoicesDTOS;
using erp.DTOS;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace erp.Helpers
{
    public static class PrintHelper
    {
        public static void PrintInvoice(
            InvoiceResponseDto invoice,
            UserDto user)
        {
            var doc = new FlowDocument
            {
                FlowDirection = FlowDirection.RightToLeft,
                FontFamily = new FontFamily("Segoe UI")
            };

            doc.Blocks.Add(new Paragraph(new Run($"فاتورة: {invoice.Id}")));
            doc.Blocks.Add(new Paragraph(new Run($"العميل: {user.Fullname}")));
            doc.Blocks.Add(new Paragraph(new Run($"المبلغ: {invoice.Amount}")));
            doc.Blocks.Add(new Paragraph(new Run($"المدفوع: {invoice.PaidAmount}")));
            doc.Blocks.Add(new Paragraph(new Run($"المتبقي: {invoice.RemainingAmount}")));

            Print(doc);
        }

        public static void PrintUserLedger(
            UserDto user,
            IEnumerable<InvoiceResponseDto> invoices)
        {
            var doc = new FlowDocument
            {
                FlowDirection = FlowDirection.RightToLeft
            };

            doc.Blocks.Add(new Paragraph(
                new Run($"كشف حساب العميل: {user.Fullname}"))
            { FontSize = 18 });

            foreach (var inv in invoices)
            {
                doc.Blocks.Add(new Paragraph(
                    new Run($"{inv.GeneratedDate:yyyy-MM-dd} | {inv.Amount} | {inv.RemainingAmount}")));
            }

            Print(doc);
        }

        private static void Print(FlowDocument doc)
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                doc.PageHeight = dlg.PrintableAreaHeight;
                doc.PageWidth = dlg.PrintableAreaWidth;
                dlg.PrintDocument(
                    ((IDocumentPaginatorSource)doc).DocumentPaginator,
                    "ERP Print");
            }
        }
    }

}
