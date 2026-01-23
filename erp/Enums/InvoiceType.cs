namespace erp.Enums
{
    /// <summary>
    /// Defines the invoice types in the ERP system.
    /// 
    /// CRITICAL DESIGN RULE:
    /// =====================
    /// • CustomerInvoice, CommissionInvoice, ReturnInvoice → USE OrderId to load items
    /// • SupplierInvoice → USE InvoiceCode (or embedded Items) to load items
    /// 
    /// This enum should be the SINGLE source of truth for invoice type classification.
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>
        /// فاتورة عميل - Customer Invoice
        /// Items loaded via: OrderId → Order Items
        /// </summary>
        CustomerInvoice,

        /// <summary>
        /// فاتورة عمولة - Commission/Sales Rep Invoice
        /// Items loaded via: OrderId → Order Items
        /// </summary>
        CommissionInvoice,

        /// <summary>
        /// فاتورة مرتجع - Return Invoice
        /// Items loaded via: OrderId → Order Items (returned items)
        /// </summary>
        ReturnInvoice,

        /// <summary>
        /// فاتورة مورد - Supplier Invoice
        /// Items loaded via: InvoiceCode or embedded Items (NOT OrderId)
        /// This is the ONLY exception that doesn't use OrderId.
        /// </summary>
        SupplierInvoice,

        /// <summary>
        /// Unknown or unsupported invoice type
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Extension methods for InvoiceType enum
    /// </summary>
    public static class InvoiceTypeExtensions
    {
        /// <summary>
        /// Determines if this invoice type uses OrderId for loading items.
        /// </summary>
        /// <returns>
        /// TRUE for: CustomerInvoice, CommissionInvoice, ReturnInvoice
        /// FALSE for: SupplierInvoice, Unknown
        /// </returns>
        public static bool UsesOrderId(this InvoiceType type)
        {
            return type switch
            {
                InvoiceType.CustomerInvoice => true,
                InvoiceType.CommissionInvoice => true,
                InvoiceType.ReturnInvoice => true,
                InvoiceType.SupplierInvoice => false,
                InvoiceType.Unknown => false,
                _ => false
            };
        }

        /// <summary>
        /// Determines if this invoice type uses InvoiceCode for loading items.
        /// </summary>
        /// <returns>TRUE only for SupplierInvoice</returns>
        public static bool UsesInvoiceCode(this InvoiceType type)
        {
            return type == InvoiceType.SupplierInvoice;
        }

        /// <summary>
        /// Converts string type from API to InvoiceType enum.
        /// </summary>
        public static InvoiceType ParseFromApi(string? typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
                return InvoiceType.Unknown;

            return typeString.Trim().ToLowerInvariant() switch
            {
                "customerinvoice" => InvoiceType.CustomerInvoice,
                "commissioninvoice" => InvoiceType.CommissionInvoice,
                "returninvoice" => InvoiceType.ReturnInvoice,
                "supplierinvoice" => InvoiceType.SupplierInvoice,
                _ => InvoiceType.Unknown
            };
        }

        /// <summary>
        /// Gets the Arabic display name for the invoice type.
        /// </summary>
        public static string GetArabicDisplayName(this InvoiceType type)
        {
            return type switch
            {
                InvoiceType.CustomerInvoice => "فاتورة عميل",
                InvoiceType.CommissionInvoice => "فاتورة عمولة",
                InvoiceType.ReturnInvoice => "فاتورة مرتجع",
                InvoiceType.SupplierInvoice => "فاتورة مورد",
                InvoiceType.Unknown => "غير محدد",
                _ => "غير محدد"
            };
        }

        /// <summary>
        /// Converts InvoiceType to API string value.
        /// </summary>
        public static string ToApiString(this InvoiceType type)
        {
            return type switch
            {
                InvoiceType.CustomerInvoice => "CustomerInvoice",
                InvoiceType.CommissionInvoice => "CommissionInvoice",
                InvoiceType.ReturnInvoice => "ReturnInvoice",
                InvoiceType.SupplierInvoice => "SupplierInvoice",
                _ => ""
            };
        }
    }
}
