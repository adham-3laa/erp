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

        // ==================== Statistics Properties ====================
        public decimal TotalAmount => Invoices.Sum(i => i.Amount);
        public decimal TotalPaid => Invoices.Sum(i => i.PaidAmount);
        public decimal TotalRemaining => Invoices.Sum(i => i.RemainingAmount);
        public bool HasNoInvoices => Invoices.Count == 0;

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
                    new InventoryService(),
                    _invoiceService
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
        /// <summary>
        /// Loads invoices from multiple sources (Customer & Supplier endpoints)
        /// to support users who act as both Suppliers and Customers using the same ID.
        /// </summary>
        private async Task LoadInvoices()
        {
            Invoices.Clear();

            try
            {
                var tasks = new List<Task<List<InvoiceResponseDto>>>();

                // 1. Always try to fetch Customer Invoices (Sales)
                // Endpoint: /api/Invoices/AllInvoicesForSpecificCustomerByCustomerId
                tasks.Add(GetInvoicesSafe(() => _invoiceService.GetInvoicesForCustomer(User.Id)));

                // 2. Always try to fetch Supplier Invoices (Supply)
                // Endpoint: /api/Invoices/AllInvoicesForSpecificSupplierBySupplierId
                tasks.Add(GetInvoicesSafe(() => _invoiceService.GetInvoicesForSupplier(User.Id)));

                // 3. If SalesRep, fetch their specific invoices too
                if (User.UserType == "SalesRep")
                {
                    tasks.Add(GetInvoicesSafe(() => _invoiceService.GetInvoicesForSalesRep(User.Id)));
                }

                // Wait for all requests to complete
                await Task.WhenAll(tasks);

                // Merge all lists
                var allInvoices = tasks
                    .SelectMany(t => t.Result) // Flatten results
                    .GroupBy(i => i.Id)        // Group by ID to remove duplicates
                    .Select(g => g.First())    // Take first of each group
                    .OrderByDescending(i => i.GeneratedDate) // Sort by Date DESC (Newest First)
                    .ToList();

                // Add to ObservableCollection
                foreach (var invoice in allInvoices)
                    Invoices.Add(invoice);

                // Update Statistics
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(TotalPaid));
                OnPropertyChanged(nameof(TotalRemaining));
                OnPropertyChanged(nameof(HasNoInvoices));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Critical Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper to swallow exceptions from individual endpoints (e.g. if user not found as Supplier)
        /// returning an empty list instead of crashing the whole load.
        /// </summary>
        private async Task<List<InvoiceResponseDto>> GetInvoicesSafe(Func<Task<List<InvoiceResponseDto>>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                // Log warning but don't fail
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Warning: Failed to fetch invoices from one source. {ex.Message}");
                return new List<InvoiceResponseDto>();
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
