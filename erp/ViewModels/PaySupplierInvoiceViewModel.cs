using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.Enums;
using erp.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels.Invoices
{
    /// <summary>
    /// ViewModel for paying Supplier Invoices and Supplier Return Invoices.
    /// 
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// PAYMENT ROUTING BY INVOICE TYPE:
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// 
    /// ┌───────────────────────┬────────────────────────────────────────────────────┐
    /// │ Invoice Type          │ Payment Method                                     │
    /// ├───────────────────────┼────────────────────────────────────────────────────┤
    /// │ SupplierInvoice       │ PaySupplierInvoice(GUID, amount)                   │
    /// │ SupplierReturnInvoice │ PaySupplierInvoiceByCode(CODE, amount)             │
    /// └───────────────────────┴────────────────────────────────────────────────────┘
    /// 
    /// For Supplier Return Invoices, the API accepts invoice CODE (e.g., "33")
    /// not the GUID.
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// </summary>
    public class PaySupplierInvoiceViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _service;
        private readonly InvoiceResponseDto _invoice; // Keeps reference to update UI
        private readonly Guid _invoiceId;
        private readonly int _invoiceCode;
        private readonly InvoiceType _invoiceType;

        /// <summary>
        /// Creates ViewModel for paying a Supplier or Supplier Return Invoice.
        /// </summary>
        public PaySupplierInvoiceViewModel(InvoiceResponseDto invoice)
        {
            _invoice = invoice ?? throw new ArgumentNullException(nameof(invoice));
            _invoiceId = invoice.Id;
            _invoiceCode = invoice.code;
            _invoiceType = invoice.InvoiceTypeParsed;
            
            RemainingAmount = invoice.RemainingAmount;
            
            _service = new InvoiceService();
            PayCommand = new RelayCommand(async () => await Pay(), CanPay);

            System.Diagnostics.Debug.WriteLine(
                $"[PaySupplierInvoiceVM] Created: Id={_invoiceId}, Code={_invoiceCode}, Type={_invoiceType}, Remaining={RemainingAmount}");
        }

        /// <summary>
        /// Backwards-compatible constructor for regular Supplier Invoices.
        /// Should be avoided in favor of the DTO constructor.
        /// </summary>
        public PaySupplierInvoiceViewModel(Guid invoiceId, decimal remainingAmount)
        {
            _invoiceId = invoiceId;
            RemainingAmount = remainingAmount;
            _invoiceType = InvoiceType.SupplierInvoice;
            // Create dummy DTO to avoid null checks, though it won't be linked to UI
            _invoice = new InvoiceResponseDto { Id = invoiceId, RemainingAmount = remainingAmount }; 
            
            _service = new InvoiceService();
            PayCommand = new RelayCommand(async () => await Pay(), CanPay);
        }

        /// <summary>
        /// Indicates if this is a Supplier Return Invoice.
        /// </summary>
        public bool IsSupplierReturn => _invoiceType.IsSupplierReturn();

        private decimal _remainingAmount;
        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set { _remainingAmount = value; OnPropertyChanged(); }
        }

        private decimal _paidAmount;
        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                _paidAmount = value;
                OnPropertyChanged();
                PayCommand.NotifyCanExecuteChanged();
            }
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                PayCommand.NotifyCanExecuteChanged();
            }
        }

        public RelayCommand PayCommand { get; }

        private bool CanPay()
        {
            return !IsBusy && PaidAmount > 0 && PaidAmount <= RemainingAmount;
        }

        private async Task Pay()
        {
            // Customize confirmation message
            var confirmMessage = IsSupplierReturn
                ? $"هل أنت متأكد من تسوية مبلغ {PaidAmount:N2} لفاتورة مرتجع المورد؟"
                : $"هل أنت متأكد من دفع مبلغ {PaidAmount:N2} للمورد؟";

            if (!erp.Views.Shared.ThemedDialog.ShowConfirmation(null, "تأكيد الدفع", confirmMessage, "نعم", "لا"))
            {
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = null;
                SuccessMessage = null;

                if (PaidAmount <= 0)
                {
                    ErrorMessage = "أدخل مبلغ صحيح أكبر من صفر";
                    return;
                }

                if (PaidAmount > RemainingAmount)
                {
                    ErrorMessage = "المبلغ المدخل أكبر من المبلغ المتبقي";
                    return;
                }

                // ═══════════════════════════════════════════════════════════════
                // CRITICAL: Route to correct API method based on invoice type
                // ═══════════════════════════════════════════════════════════════
                
                if (IsSupplierReturn)
                {
                    // Supplier Return Invoice: Use invoice CODE
                    if (_invoiceCode <= 0)
                    {
                        ErrorMessage = "كود الفاتورة غير صالح - لا يمكن إتمام الدفع";
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine(
                        $"[PaySupplierInvoiceVM] Paying SupplierReturn by CODE: {_invoiceCode}");

                    var result = await _service.PaySupplierInvoiceByCode(_invoiceCode, PaidAmount);
                    
                    // Use server response for accurate remaining amount
                    RemainingAmount = result.RemainingAmount;
                    SuccessMessage = "تمت تسوية فاتورة مرتجع المورد بنجاح";

                    // ═══════════════════════════════════════════════════════════════
                    // CRITICAL UI UPDATE: Update original DTO
                    // ═══════════════════════════════════════════════════════════════
                    _invoice.PaidAmount = result.PaidAmount;
                    _invoice.RemainingAmount = result.RemainingAmount;
                }
                else
                {
                    // Regular Supplier Invoice: Use GUID
                    System.Diagnostics.Debug.WriteLine(
                        $"[PaySupplierInvoiceVM] Paying Supplier by GUID: {_invoiceId}");

                    await _service.PaySupplierInvoice(_invoiceId, PaidAmount);
                    
                    // Update locally for regular invoices
                    RemainingAmount -= PaidAmount;
                    SuccessMessage = "تمت عملية الدفع للمورد بنجاح";

                    // Update DTO locally since regular API doesn't return updated amounts
                     _invoice.PaidAmount += PaidAmount;
                     _invoice.RemainingAmount -= PaidAmount;
                }
                
                PaidAmount = 0;

                System.Diagnostics.Debug.WriteLine(
                    $"[PaySupplierInvoiceVM] Payment SUCCESS. New remaining: {RemainingAmount}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"فشل الدفع: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(
                    $"[PaySupplierInvoiceVM] Payment FAILED: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}

