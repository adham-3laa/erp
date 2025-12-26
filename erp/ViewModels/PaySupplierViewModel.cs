using CommunityToolkit.Mvvm.Input;
using erp.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace erp.ViewModels
{
    public class PaySupplierViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _invoiceService;

        public PaySupplierViewModel()
        {
            _invoiceService = new InvoiceService();
            PayCommand = new RelayCommand(async () => await Pay(), CanPay);
        }

        // ================= Inputs =================

        private string _supplierInvoiceId;
        public string SupplierInvoiceId
        {
            get => _supplierInvoiceId;
            set
            {
                _supplierInvoiceId = value;
                OnPropertyChanged();
                PayCommand.NotifyCanExecuteChanged();
            }
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

        // ================= UI =================

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                PayCommand.NotifyCanExecuteChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isSuccess = false;
        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                _isSuccess = value;
                OnPropertyChanged();
            }
        }

        // ================= Command =================

        public RelayCommand PayCommand { get; }

        private bool CanPay()
        {
            return !IsLoading
                   && PaidAmount > 0
                   && Guid.TryParse(SupplierInvoiceId, out _);
        }

        // ================= Logic =================

        private async Task Pay()
        {
            // 👈 مهم جدًا
            IsSuccess = false;
            ErrorMessage = null;

            if (!Guid.TryParse(SupplierInvoiceId, out var invoiceId))
            {
                ErrorMessage = "رقم فاتورة المورد غير صحيح";
                return;
            }

            if (PaidAmount <= 0)
            {
                ErrorMessage = "من فضلك أدخل مبلغ صحيح";
                return;
            }

            try
            {
                IsLoading = true;

                await _invoiceService.PaySupplierInvoice(invoiceId, PaidAmount);

                PaidAmount = 0;

                // ✅ هنا فقط
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                IsSuccess = false;
            }
            finally
            {
                IsLoading = false;
            }
        }



        // ================= INotify =================

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
