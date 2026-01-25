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
                if (_paidAmount != value)
                {
                    _paidAmount = value;
                    OnPropertyChanged();

                    // Sync text only if actual value mismatch (supports programmatic reset)
                    if (!decimal.TryParse(_paidAmountText, out var currentVal) || currentVal != value)
                    {
                        _paidAmountText = value == 0 ? "" : value.ToString();
                        OnPropertyChanged(nameof(PaidAmountText));
                    }

                    PayCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _paidAmountText = "";
        public string PaidAmountText
        {
            get => _paidAmountText;
            set
            {
                if (_paidAmountText != value)
                {
                    _paidAmountText = value;
                    OnPropertyChanged();

                    if (decimal.TryParse(value, out var val))
                    {
                        _paidAmount = val;
                    }
                    else
                    {
                        _paidAmount = 0;
                    }
                    OnPropertyChanged(nameof(PaidAmount));
                    PayCommand.NotifyCanExecuteChanged();
                }
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
