namespace erp.Enums
{
    /// <summary>
    /// Defines the invoice types in the ERP system.
    /// 
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// CRITICAL DESIGN RULE - INVOICE DETAILS LOADING STRATEGY:
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// 
    /// ┌───────────────────────┬────────────────────────────────────────────────────┐
    /// │ Invoice Type          │ Items Loading Strategy                             │
    /// ├───────────────────────┼────────────────────────────────────────────────────┤
    /// │ CustomerInvoice       │ OrderId/OrderCode → GetOrderItemsByOrderCode       │
    /// │ CommissionInvoice     │ OrderId/OrderCode → GetOrderItemsByOrderCode       │
    /// │ ReturnInvoice         │ OrderId/OrderCode → GetOrderItemsByOrderCode       │
    /// │ SupplierInvoice       │ InvoiceCode → GetSupplierInviceProductsByInvoicCode│
    /// │ SupplierReturnInvoice │ InvoiceCode → GetAllProductsInSpecificReturn...    │
    /// └───────────────────────┴────────────────────────────────────────────────────┘
    /// 
    /// This enum is the SINGLE source of truth for invoice type classification.
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>
        /// فاتورة عميل - Customer Invoice
        /// Items loaded via: OrderId/OrderCode → Order Items
        /// </summary>
        CustomerInvoice,

        /// <summary>
        /// فاتورة عمولة - Commission/Sales Rep Invoice
        /// Items loaded via: OrderId/OrderCode → Order Items
        /// </summary>
        CommissionInvoice,

        /// <summary>
        /// فاتورة مرتجع - Return Invoice (Customer Return)
        /// Items loaded via: OrderId/OrderCode → Order Items (returned items)
        /// </summary>
        ReturnInvoice,

        /// <summary>
        /// فاتورة مورد - Supplier Invoice
        /// Items loaded via: InvoiceCode (or embedded Items) 
        /// Endpoint: GET /api/Invoices/GetSupplierInviceProductsByInvoicCode
        /// </summary>
        SupplierInvoice,

        /// <summary>
        /// فاتورة مرتجع مورد - Supplier Return Invoice
        /// Items loaded via: InvoiceCode ONLY
        /// Endpoint: GET /api/Returns/GetAllProductsInSpecificReturnSupplierInvoice?invoiceCode={code}
        /// 
        /// ═══════════════════════════════════════════════════════════════════
        /// MANDATORY RULE: This invoice type MUST use the dedicated endpoint
        /// and ONLY the invoiceCode parameter. Do NOT use OrderId, CustomerId,
        /// or SalesRepId. This is an ERP-critical financial document.
        /// ═══════════════════════════════════════════════════════════════════
        /// </summary>
        SupplierReturnInvoice,

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
        /// Determines if this invoice type uses OrderId/OrderCode for loading items.
        /// </summary>
        /// <returns>
        /// TRUE for: CustomerInvoice, CommissionInvoice, ReturnInvoice
        /// FALSE for: SupplierInvoice, SupplierReturnInvoice, Unknown
        /// </returns>
        public static bool UsesOrderId(this InvoiceType type)
        {
            return type switch
            {
                InvoiceType.CustomerInvoice => true,
                InvoiceType.CommissionInvoice => true,
                InvoiceType.ReturnInvoice => true,
                InvoiceType.SupplierInvoice => false,
                InvoiceType.SupplierReturnInvoice => false, // Uses InvoiceCode, NOT OrderId
                InvoiceType.Unknown => false,
                _ => false
            };
        }

        /// <summary>
        /// Determines if this invoice type uses InvoiceCode for loading items.
        /// </summary>
        /// <returns>TRUE for: SupplierInvoice, SupplierReturnInvoice</returns>
        public static bool UsesInvoiceCode(this InvoiceType type)
        {
            return type == InvoiceType.SupplierInvoice || 
                   type == InvoiceType.SupplierReturnInvoice;
        }

        /// <summary>
        /// Determines if this invoice type is a Supplier Return Invoice.
        /// Used to route to the correct endpoint.
        /// </summary>
        public static bool IsSupplierReturn(this InvoiceType type)
        {
            return type == InvoiceType.SupplierReturnInvoice;
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
                "supplierreturninvoice" => InvoiceType.SupplierReturnInvoice,
                "returntoSupplierinvoice" => InvoiceType.SupplierReturnInvoice, // Alternative API naming
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
                InvoiceType.SupplierReturnInvoice => "فاتورة مرتجع مورد",
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
                InvoiceType.SupplierReturnInvoice => "SupplierReturnInvoice",
                _ => ""
            };
        }
    }
}
