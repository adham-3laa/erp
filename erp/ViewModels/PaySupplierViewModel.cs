using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace erp.ViewModels
{
    public class PaySupplierViewModel : INotifyPropertyChanged
    {
        private readonly PaymentsService _paymentsService;

        public PaySupplierViewModel()
        {
            _paymentsService = new PaymentsService();
            PayCommand = new RelayCommand(async () => await Pay());
        }

        // ================= Inputs =================

        private string _supplierInvoiceId;
        public string SupplierInvoiceId
        {
            get => _supplierInvoiceId;
            set { _supplierInvoiceId = value; OnPropertyChanged(); }
        }

        private decimal _paidAmount;
        public decimal PaidAmount
        {
            get => _paidAmount;
            set { _paidAmount = value; OnPropertyChanged(); }
        }

        // ================= Result =================

        private PaySupplierResponseDto _result;
        public PaySupplierResponseDto Result
        {
            get => _result;
            set { _result = value; OnPropertyChanged(); }
        }

        // ================= UI =================

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ================= Command =================

        public RelayCommand PayCommand { get; }

        private async Task Pay()
        {
            if (!Guid.TryParse(SupplierInvoiceId, out var invoiceId))
                return;

            try
            {
                IsLoading = true;

                Result = await _paymentsService.PayToSupplier(
                    invoiceId,
                    PaidAmount);
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
