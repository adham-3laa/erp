using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace erp.ViewModels.Invoices
{
    public class PaymentsFromCustomersViewModel : INotifyPropertyChanged
    {
        private readonly InvoicePaymentService _service;

        public PaymentsFromCustomersViewModel()
        {
            _service = new InvoicePaymentService();
            LoadPaymentCommand = new RelayCommand(async () => await LoadPayment());
        }

        // ================= Inputs =================

        private string _targetType;
        public string TargetType
        {
            get => _targetType;
            set { _targetType = value; OnPropertyChanged(); }
        }

        private string _orderId;
        public string OrderId
        {
            get => _orderId;
            set { _orderId = value; OnPropertyChanged(); }
        }

        private decimal _paidAmount;
        public decimal PaidAmount
        {
            get => _paidAmount;
            set { _paidAmount = value; OnPropertyChanged(); }
        }

        // ================= Result =================

        private PaidAmountResponseDto _result;
        public PaidAmountResponseDto Result
        {
            get => _result;
            set { _result = value; OnPropertyChanged(); }
        }

        // ================= Command =================

        public RelayCommand LoadPaymentCommand { get; }

        private async Task LoadPayment()
        {
            Result = await _service.PayedAmountFromCustomerByOrderID(
                TargetType,
                OrderId,
                PaidAmount
            );
        }

        // ================= INotify =================
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
