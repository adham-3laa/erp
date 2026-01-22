using erp.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace erp.DTOS.InvoicesDTOS
{
    /// <summary>
    /// DTO representing an invoice from the API.
    /// 
    /// CRITICAL DESIGN RULE - Invoice Details Loading:
    /// ================================================
    /// • CustomerInvoice, CommissionInvoice, ReturnInvoice → Items loaded via OrderId
    /// • SupplierInvoice → Items loaded via InvoiceCode or embedded Items list
    /// 
    /// Use InvoiceTypeParsed and the UsesOrderId()/UsesInvoiceCode() extension methods
    /// to determine the correct loading strategy.
    /// </summary>
    public class InvoiceResponseDto : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("code")]
        public int code { get; set; }

        /// <summary>
        /// Raw invoice type string from API.
        /// Values: CustomerInvoice, CommissionInvoice, ReturnInvoice, SupplierInvoice
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Parsed invoice type enum for type-safe operations.
        /// Use this property with InvoiceTypeExtensions for loading strategy decisions.
        /// </summary>
        [JsonIgnore]
        public InvoiceType InvoiceTypeParsed => InvoiceTypeExtensions.ParseFromApi(Type);

        /// <summary>
        /// Determines if this invoice's items should be loaded via OrderId.
        /// TRUE for: CustomerInvoice, CommissionInvoice, ReturnInvoice
        /// FALSE for: SupplierInvoice
        /// </summary>
        [JsonIgnore]
        public bool ShouldLoadByOrderId => InvoiceTypeParsed.UsesOrderId();

        /// <summary>
        /// Determines if this invoice's items should be loaded via InvoiceCode.
        /// TRUE only for: SupplierInvoice
        /// </summary>
        [JsonIgnore]
        public bool ShouldLoadByInvoiceCode => InvoiceTypeParsed.UsesInvoiceCode();

        /// <summary>
        /// Recipient name - for Customer, Commission, Return invoices.
        /// </summary>
        [JsonPropertyName("recipientname")]
        public string? RecipientName { get; set; }

        /// <summary>
        /// Supplier name - for Supplier invoices.
        /// </summary>
        [JsonPropertyName("suppliername")]
        public string? SupplierName { get; set; }

        /// <summary>
        /// Supplier ID - for Supplier invoices.
        /// </summary>
        [JsonPropertyName("supplierid")]
        public string? SupplierId { get; set; }

        /// <summary>
        /// Display name - shows the appropriate name based on invoice type.
        /// For SupplierInvoice: returns SupplierName
        /// For other invoices: returns RecipientName
        /// </summary>
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (InvoiceTypeParsed == InvoiceType.SupplierInvoice)
                {
                    return !string.IsNullOrWhiteSpace(SupplierName) ? SupplierName : "غير محدد";
                }
                return !string.IsNullOrWhiteSpace(RecipientName) ? RecipientName : "غير محدد";
            }
        }


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

        /// <summary>
        /// OrderId linked to this invoice (GUID).
        /// Used to identify the order but NOT for loading items.
        /// </summary>
        [JsonPropertyName("orderid")]
        public Guid? OrderId { get; set; }

        /// <summary>
        /// Order Code (sequential integer) linked to this invoice.
        /// CRITICAL: This is what the API uses to load order items.
        /// Endpoint: /api/Returns/OrderItemsByOrderId?orderCode={orderCode}
        /// </summary>
        [JsonPropertyName("ordercode")]
        public int? OrderCode { get; set; }


        /// <summary>
        /// Embedded items list - used ONLY for SupplierInvoice.
        /// For all other invoice types, items are loaded via OrderId.
        /// </summary>
        public List<InvoiceItemDto>? Items { get; set; }

        /// <summary>
        /// Gets the Arabic display name for this invoice type.
        /// </summary>
        [JsonIgnore]
        public string TypeDisplayName => InvoiceTypeParsed.GetArabicDisplayName();

        // ================= INotifyPropertyChanged =================
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
