using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels.Invoices
{
    public class PayInvoiceByOrderViewModel : INotifyPropertyChanged
    {
        private readonly InvoicePaymentService _service;

        public PayInvoiceByOrderViewModel(InvoiceResponseDto invoice)
        {
            _service = new InvoicePaymentService();

            OrderId = invoice.OrderId?.ToString();
            RemainingAmount = invoice.RemainingAmount;

            TargetType = invoice.Type switch
            {
                "CommissionInvoice" => "SalesRep",
                _ => "Customer"
            };

            PayCommand = new RelayCommand(async () => await Pay(), CanPay);
        }

        public string OrderId { get; }
        public string TargetType { get; }

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

        private PaidAmountResponseDto _result;
        public PaidAmountResponseDto Result
        {
            get => _result;
            set { _result = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }

        public RelayCommand PayCommand { get; }

        private bool CanPay()
            => PaidAmount > 0 && PaidAmount <= RemainingAmount && !IsLoading;

        private async Task Pay()
        {
            if (!erp.Views.Shared.ThemedDialog.ShowConfirmation(null, "تأكيد الدفع", $"هل أنت متأكد من دفع مبلغ {PaidAmount:N2}؟", "نعم", "لا"))
            {
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                SuccessMessage = null;

                Result = await _service.PayedAmountFromCustomerByOrderID(
                    TargetType,
                    OrderId,
                    PaidAmount
                );

                RemainingAmount = Result.RemainingAmount;
                PaidAmount = 0;
                SuccessMessage = "تمت عملية الدفع بنجاح";
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"حدث خطأ أثناء الدفع: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                // CommandManager.InvalidateRequerySuggested(); // For WPF commands update
                PayCommand.NotifyCanExecuteChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
