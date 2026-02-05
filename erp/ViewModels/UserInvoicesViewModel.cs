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

        // ==================== Statistics Properties from API ====================
        
        // ==================== Main Stats (Sales or Supply) ====================
        private string _mainStatsTitle = "الإحصائيات الأساسية";
        public string MainStatsTitle
        {
            get => _mainStatsTitle;
            set { _mainStatsTitle = value; OnPropertyChanged(); }
        }

        private int _mainCount;
        public int MainCount
        {
            get => _mainCount;
            set { _mainCount = value; OnPropertyChanged(); }
        }

        private decimal _mainAmount;
        public decimal MainAmount
        {
            get => _mainAmount;
            set { _mainAmount = value; OnPropertyChanged(); }
        }

        private decimal _mainPaid;
        public decimal MainPaid
        {
            get => _mainPaid;
            set { _mainPaid = value; OnPropertyChanged(); }
        }

        private decimal _mainRemaining;
        public decimal MainRemaining
        {
            get => _mainRemaining;
            set { _mainRemaining = value; OnPropertyChanged(); }
        }

        // ==================== Returns Stats ====================
        private int _returnsCount;
        public int ReturnsCount
        {
            get => _returnsCount;
            set { _returnsCount = value; OnPropertyChanged(); }
        }

        private decimal _returnsAmount;
        public decimal ReturnsAmount
        {
            get => _returnsAmount;
            set { _returnsAmount = value; OnPropertyChanged(); }
        }

        private decimal _returnsPaid;
        public decimal ReturnsPaid
        {
            get => _returnsPaid;
            set { _returnsPaid = value; OnPropertyChanged(); }
        }

        private decimal _returnsRemaining;
        public decimal ReturnsRemaining
        {
            get => _returnsRemaining;
            set { _returnsRemaining = value; OnPropertyChanged(); }
        }

        public bool HasNoInvoices => Invoices.Count == 0;
        public bool IsCustomer => User?.UserType?.ToLower().Contains("customer") ?? false;
        public bool IsSupplier => User?.UserType?.ToLower().Contains("supplier") ?? false;

        public RelayCommand PrintAllCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand<InvoiceResponseDto> PrintSingleCommand { get; }

        public UserInvoicesViewModel(UserDto user)
        {
            User = user;

            // Set Initial Title
            if (IsSupplier) MainStatsTitle = "إحصائيات التوريد";
            else MainStatsTitle = "إحصائيات المبيعات";

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
        /// Loads invoices based on user type (Customer or Supplier).
        /// Uses the new API methods that return invoices WITH statistics.
        /// </summary>
        private async Task LoadInvoices()
        {
            Invoices.Clear();

            try
            {
                // Determine user type and call appropriate endpoint
                var userType = User.UserType?.ToLowerInvariant() ?? "";
                
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Loading for user: {User.Fullname}, Type: {User.UserType}, Id: {User.Id}");

                if (userType.Contains("supplier") || userType.Contains("مورد"))
                {
                    await LoadSupplierInvoices();
                }
                else if (userType.Contains("customer") || userType.Contains("عميل"))
                {
                    await LoadCustomerInvoices();
                }
                else if (userType.Contains("salesrep") || userType.Contains("مندوب"))
                {
                    // SalesRep - try to load their invoices
                    await LoadSalesRepInvoices();
                }
                else
                {
                    // Unknown type - try both endpoints
                    await LoadBothEndpoints();
                }

                // Update HasNoInvoices
                OnPropertyChanged(nameof(HasNoInvoices));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Critical Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads invoices for a customer using the new API with statistics.
        /// </summary>
        private async Task LoadCustomerInvoices()
        {
            try
            {
                MainStatsTitle = "إحصائيات المبيعات";
                var result = await _invoiceService.GetCustomerInvoicesWithStats(User.Id);

                // Sales Stats
                MainCount = result.SalesCount;
                MainAmount = result.SalesTotalAmount;
                MainPaid = result.SalesPaidAmount;
                MainRemaining = result.SalesRemainingAmount;

                // Returns Stats
                ReturnsCount = result.ReturnsCount;
                ReturnsAmount = result.ReturnsTotalAmount;
                ReturnsPaid = result.ReturnsPaidAmount;
                ReturnsRemaining = result.ReturnsRemainingAmount;

                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Customer Sales Stats - Count: {MainCount}, Amount: {MainAmount}");
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Customer Returns Stats - Count: {ReturnsCount}, Amount: {ReturnsAmount}");

                // Add invoices to collection (sorted by date DESC)
                var sortedInvoices = result.Invoices
                    .OrderByDescending(i => i.GeneratedDate)
                    .ToList();

                foreach (var invoice in sortedInvoices)
                    Invoices.Add(invoice);

                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Loaded {Invoices.Count} customer invoices");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Failed to load customer invoices: {ex.Message}");
                // Fallback to old method
                await LoadFallback();
            }
        }

        /// <summary>
        /// Loads invoices for a supplier using the new API with statistics.
        /// </summary>
        private async Task LoadSupplierInvoices()
        {
            try
            {
                MainStatsTitle = "إحصائيات التوريد";
                var result = await _invoiceService.GetSupplierInvoicesWithStats(User.Id);

                // Supply Stats
                MainCount = result.SupplyCount;
                MainAmount = result.SupplyTotalAmount;
                MainPaid = result.SupplyPaidAmount;
                MainRemaining = result.SupplyRemainingAmount;

                // Returns Stats
                ReturnsCount = result.ReturnsCount;
                ReturnsAmount = result.ReturnsTotalAmount;
                ReturnsPaid = result.ReturnsPaidAmount;
                ReturnsRemaining = result.ReturnsRemainingAmount;

                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Supplier Supply Stats - Count: {MainCount}, Amount: {MainAmount}");
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Supplier Returns Stats - Count: {ReturnsCount}, Amount: {ReturnsAmount}");

                // Add invoices to collection (sorted by date DESC)
                var sortedInvoices = result.Invoices
                    .OrderByDescending(i => i.GeneratedDate)
                    .ToList();

                foreach (var invoice in sortedInvoices)
                    Invoices.Add(invoice);

                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Loaded {Invoices.Count} supplier invoices");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Failed to load supplier invoices: {ex.Message}");
                // Fallback to old method
                await LoadFallback();
            }
        }

        /// <summary>
        /// Loads invoices for a sales rep.
        /// </summary>
        private async Task LoadSalesRepInvoices()
        {
            try
            {
                MainStatsTitle = "إحصائيات المندوب";
                var invoices = await _invoiceService.GetInvoicesForSalesRep(User.Id);

                // Calculate stats from invoices (Basic fallback)
                MainCount = invoices.Count;
                MainAmount = invoices.Sum(i => i.Amount);
                MainPaid = invoices.Sum(i => i.PaidAmount);
                MainRemaining = invoices.Sum(i => i.RemainingAmount);

                // No returns stats calculator for SalesRep here yet
                ReturnsCount = 0;
                ReturnsAmount = 0;
                ReturnsPaid = 0;
                ReturnsRemaining = 0;

                var sortedInvoices = invoices
                    .OrderByDescending(i => i.GeneratedDate)
                    .ToList();

                foreach (var invoice in sortedInvoices)
                    Invoices.Add(invoice);

                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Loaded {Invoices.Count} salesrep invoices");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Failed to load salesrep invoices: {ex.Message}");
            }
        }

        /// <summary>
        /// Tries both customer and supplier endpoints for unknown user types.
        /// </summary>
        private async Task LoadBothEndpoints()
        {
            System.Diagnostics.Debug.WriteLine("[UserInvoicesVM] Unknown user type - trying both endpoints");

            // Try customer first
            try
            {
                var customerResult = await _invoiceService.GetCustomerInvoicesWithStats(User.Id);
                if (customerResult.Invoices.Any())
                {
                    MainStatsTitle = "إحصائيات العميل";
                    
                    MainCount = customerResult.SalesCount;
                    MainAmount = customerResult.SalesTotalAmount;
                    MainPaid = customerResult.SalesPaidAmount;
                    MainRemaining = customerResult.SalesRemainingAmount;
                    
                    ReturnsCount = customerResult.ReturnsCount;
                    ReturnsAmount = customerResult.ReturnsTotalAmount;
                    ReturnsPaid = customerResult.ReturnsPaidAmount;
                    ReturnsRemaining = customerResult.ReturnsRemainingAmount;

                    foreach (var invoice in customerResult.Invoices.OrderByDescending(i => i.GeneratedDate))
                        Invoices.Add(invoice);

                    System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Found {Invoices.Count} as customer");
                    return;
                }
            }
            catch { /* Ignore and try supplier */ }

            // Try supplier
            try
            {
                var supplierResult = await _invoiceService.GetSupplierInvoicesWithStats(User.Id);
                if (supplierResult.Invoices.Any())
                {
                    MainStatsTitle = "إحصائيات المورد";

                    MainCount = supplierResult.SupplyCount;
                    MainAmount = supplierResult.SupplyTotalAmount;
                    MainPaid = supplierResult.SupplyPaidAmount;
                    MainRemaining = supplierResult.SupplyRemainingAmount;

                    ReturnsCount = supplierResult.ReturnsCount;
                    ReturnsAmount = supplierResult.ReturnsTotalAmount;
                    ReturnsPaid = supplierResult.ReturnsPaidAmount;
                    ReturnsRemaining = supplierResult.ReturnsRemainingAmount;

                    foreach (var invoice in supplierResult.Invoices.OrderByDescending(i => i.GeneratedDate))
                        Invoices.Add(invoice);

                    System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Found {Invoices.Count} as supplier");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Both endpoints failed: {ex.Message}");
            }
        }


        /// <summary>
        /// Fallback method using old API calls (calculates stats locally).
        /// </summary>
        private async Task LoadFallback()
        {
            try
            {
                var tasks = new List<Task<List<InvoiceResponseDto>>>
                {
                    GetInvoicesSafe(() => _invoiceService.GetInvoicesForCustomer(User.Id)),
                    GetInvoicesSafe(() => _invoiceService.GetInvoicesForSupplier(User.Id))
                };

                await Task.WhenAll(tasks);

                var allInvoices = tasks
                    .SelectMany(t => t.Result)
                    .GroupBy(i => i.Id)
                    .Select(g => g.First())
                    .OrderByDescending(i => i.GeneratedDate)
                    .ToList();

                foreach (var invoice in allInvoices)
                    Invoices.Add(invoice);

                // Calculate stats locally
                MainCount = Invoices.Count;
                MainAmount = Invoices.Sum(i => i.Amount);
                MainPaid = Invoices.Sum(i => i.PaidAmount);
                MainRemaining = Invoices.Sum(i => i.RemainingAmount);
                
                // Zero out returns stats for fallback
                ReturnsCount = 0;
                ReturnsAmount = 0;
                ReturnsPaid = 0;
                ReturnsRemaining = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserInvoicesVM] Fallback also failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper to swallow exceptions from individual endpoints.
        /// </summary>
        private async Task<List<InvoiceResponseDto>> GetInvoicesSafe(Func<Task<List<InvoiceResponseDto>>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
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
