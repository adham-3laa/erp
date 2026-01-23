using CommunityToolkit.Mvvm.Input;
using erp.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels.Invoices
{
    public class PaySupplierInvoiceViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _service;
        private readonly Guid _invoiceId;

        public PaySupplierInvoiceViewModel(Guid invoiceId, decimal remainingAmount)
        {
            _invoiceId = invoiceId;
            RemainingAmount = remainingAmount;
            _service = new InvoiceService();
            PayCommand = new RelayCommand(async () => await Pay(), CanPay);
        }

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
            if (!erp.Views.Shared.ThemedDialog.ShowConfirmation(null, "تأكيد الدفع", $"هل أنت متأكد من دفع مبلغ {PaidAmount:N2} للمورد؟", "نعم", "لا"))
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

                // Call API
                await _service.PaySupplierInvoice(_invoiceId, PaidAmount);
                
                // Update UI locally
                RemainingAmount -= PaidAmount;
                SuccessMessage = "تمت عملية الدفع للمورد بنجاح";
                PaidAmount = 0;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"فشل الدفع: {ex.Message}";
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
