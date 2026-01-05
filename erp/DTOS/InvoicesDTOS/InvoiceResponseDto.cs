using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace erp.DTOS.InvoicesDTOS
{
    public class InvoiceResponseDto : INotifyPropertyChanged
    {
        public Guid Id { get; set; }

        // CustomerInvoice | SupplierInvoice
        public string? Type { get; set; }

        public string? RecipientName { get; set; }

        public decimal Amount { get; set; }

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
                }
            }
        }

        private decimal _remainingAmount;
        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set
            {
                if (_remainingAmount != value)
                {
                    _remainingAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime GeneratedDate { get; set; }

        // موجودة في Customer Invoice
        public Guid? OrderId { get; set; }

        // ✅ جديدة – موجودة في Supplier Invoice
        public List<InvoiceItemDto>? Items { get; set; }

        // ================= INotifyPropertyChanged =================
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
