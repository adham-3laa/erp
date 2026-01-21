using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace erp.DTOS.InvoicesDTOS
{
    public class InvoiceResponseDto : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("code")]
        public int code { get; set; }

        // CustomerInvoice | SupplierInvoice
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("recipientname")]
        public string? RecipientName { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        private decimal _paidAmount;
        [JsonPropertyName("paidamount")]
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
        [JsonPropertyName("remainingamount")]
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

        [JsonPropertyName("generateddate")]
        public DateTime GeneratedDate { get; set; }

        // موجودة في Customer Invoice
        [JsonPropertyName("orderid")]
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
