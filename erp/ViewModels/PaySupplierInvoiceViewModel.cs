using CommunityToolkit.Mvvm.Input;
using erp.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace erp.ViewModels.Invoices
{
    public class PaySupplierInvoiceViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceService _service;
        private readonly Guid _invoiceId;

        public PaySupplierInvoiceViewModel(Guid invoiceId)
        {
            _invoiceId = invoiceId;
            _service = new InvoiceService();
            PayCommand = new RelayCommand(async () => await Pay(), CanPay);
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
            return !IsBusy && PaidAmount > 0;
        }

        private async Task Pay()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;

                if (PaidAmount <= 0)
                {
                    ErrorMessage = "أدخل مبلغ صحيح";
                    return;
                }

                await _service.PaySupplierInvoice(_invoiceId, PaidAmount);
                PaidAmount = 0;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
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
