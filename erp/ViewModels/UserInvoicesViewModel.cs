using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.DTOS;
using erp.Services;
using erp.Helpers;
using erp.Printing;
using System.Diagnostics;
using System.IO;
using QuestPDF.Fluent;

namespace erp.ViewModels
{
    public class UserInvoicesViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService = new();
        private readonly InvoicePrintService _invoicePrintService;

        public UserDto User { get; }

        public ObservableCollection<InvoiceResponseDto> Invoices { get; }
            = new();

        public RelayCommand PrintAllCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand<InvoiceResponseDto> PrintSingleCommand { get; }

        public UserInvoicesViewModel(UserDto user)
        {
            User = user;

            PrintAllCommand = new RelayCommand(PrintAll);
            PrintSingleCommand = new RelayCommand<InvoiceResponseDto>(PrintSingle);

            _invoicePrintService =
                new InvoicePrintService(
                    new OrdersService(App.Api),
                    new InventoryService()
                );

            BackCommand = new RelayCommand(
                () => NavigationService.NavigateToUsers()
            );

            _ = LoadInvoices();
        }

        /// <summary>
        /// ✅ هنا التعديل الأساسي:
        /// بدل ما نجيب فواتير "بالنوع" لكل الناس من /api/Invoices/list
        /// بنجيب فواتير اليوزر نفسه من endpoints المتخصصة حسب نوعه.
        /// </summary>
        private async Task LoadInvoices()
        {
            Invoices.Clear();

            try
            {
                List<InvoiceResponseDto> list = new();

                switch (User.UserType)
                {
                    case "Customer":
                        // ✅ فواتير عميل محدد
                        list = await _invoiceService.GetInvoicesForCustomer(User.Id);
                        break;

                    case "Supplier":
                        // ✅ فواتير مورد محدد
                        list = await _invoiceService.GetInvoicesForSupplier(User.Id);
                        break;

                    case "SalesRep":
                        // ✅ فواتير مندوب محدد
                        // لو عندك SalesRepId مختلف عن User.Id، بدّل هنا
                        list = await _invoiceService.GetInvoicesForSalesRep(User.Id);
                        break;

                    default:
                        // لو نوع غير معروف/أدمن: خليها فاضية (أو ودّيه لصفحة إدارة الفواتير)
                        list = new List<InvoiceResponseDto>();
                        break;
                }

                foreach (var invoice in list)
                    Invoices.Add(invoice);
            }
            catch
            {
                // تقدر تضيف MessageBox لو تحب
                // MessageBox.Show(ex.Message);
            }
        }

        private async void PrintSingle(InvoiceResponseDto invoice)
        {
            var printable = await _invoicePrintService
                .BuildPrintableInvoiceAsync(User, invoice);

            if (printable == null || !printable.Items.Any())
                return;

            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Invoice_{invoice.Id}.pdf"
            );

            var doc = new InvoiceWithItemsPdfDocument(printable);
            doc.GeneratePdf(path);

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });
        }

        private void PrintAll()
        {
            var filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Ledger_{User.Fullname}.pdf"
            );

            var doc = new LedgerPdfDocument(User, Invoices);
            doc.GeneratePdf(filePath);

            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }
    }
}
