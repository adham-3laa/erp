# Order-Centric Invoice Details System

## Architecture Design Document

---

## 📌 Core Rule (MUST BE FOLLOWED)

```
Invoice Details = Order Details (except for Supplier Invoices)
```

Every invoice displays the items from its related order. The `OrderId` is the **single source of truth** for loading invoice details.

---

## 🧾 Invoice Types & Loading Strategies

| Invoice Type | Identifier Used | Items Source | Description |
|--------------|-----------------|--------------|-------------|
| **CustomerInvoice** | `OrderId` | Order Items | Customer order items |
| **CommissionInvoice** | `OrderId` | Order Items | Sales rep commission on order |
| **ReturnInvoice** | `OrderId` | Order Items | Returned order items |
| **SupplierInvoice** | `InvoiceCode` | Embedded `Items[]` | ⚠️ EXCEPTION - Uses code, not order |

---

## 🆔 Identifier Types

| Identifier | Type | Purpose |
|------------|------|---------|
| `InvoiceId` | `Guid` (string) | Unique invoice identifier |
| `OrderId` | `Guid` (string) | Order identifier (for reference only) |
| `OrderCode` | `int` | **THE source of truth** - Used by API to load items |
| `InvoiceCode` | `int` | Sequential, human-readable - **Only for supplier invoices** |

### ⚠️ CRITICAL: API Endpoint Uses OrderCode (NOT OrderId)

```
GET /api/Returns/OrderItemsByOrderId?orderCode={orderCode}
```

The API parameter name is misleading (`OrderItemsByOrderId`), but it actually requires:
- `orderCode` - **integer** (e.g., `12`)
- NOT `orderId` (GUID)

---

## 📁 File Structure

```
erp/
├── Enums/
│   └── InvoiceType.cs              # ✅ Type-safe invoice classification
├── DTOS/
│   └── InvoicesDTOS/
│       └── InvoiceResponseDto.cs   # ✅ Added OrderCode property
├── Services/
│   └── OrdersService.cs            # ✅ Added GetOrderItemsByOrderCodeAsync
└── ViewModels/
    └── InvoiceDetailsViewModel.cs  # ✅ Uses OrderCode for API calls
```


---

## 🔧 Implementation Details

### 1. InvoiceType Enum (`Enums/InvoiceType.cs`)

```csharp
public enum InvoiceType
{
    CustomerInvoice,    // Uses OrderId
    CommissionInvoice,  // Uses OrderId
    ReturnInvoice,      // Uses OrderId
    SupplierInvoice,    // Uses InvoiceCode (EXCEPTION)
    Unknown
}
```

**Extension Methods:**
- `UsesOrderId()` - Returns `true` for Customer, Commission, Return invoices
- `UsesInvoiceCode()` - Returns `true` only for Supplier invoices
- `ParseFromApi(string)` - Converts API string to enum
- `GetArabicDisplayName()` - Returns Arabic localized name
- `ToApiString()` - Converts enum to API string value

### 2. InvoiceResponseDto Helpers

```csharp
// New properties added to InvoiceResponseDto:
public InvoiceType InvoiceTypeParsed => InvoiceTypeExtensions.ParseFromApi(Type);
public bool ShouldLoadByOrderId => InvoiceTypeParsed.UsesOrderId();
public bool ShouldLoadByInvoiceCode => InvoiceTypeParsed.UsesInvoiceCode();
```

### 3. InvoiceDetailsViewModel Logic

```
LoadAsync()
├── If SupplierInvoice:
│   └── LoadSupplierInvoiceItemsAsync()
│       └── API: GET /api/Invoices/GetSupplierInviceProductsByInvoicCode?invoiceCode={code}
│       └── Fallback: embedded Invoice.Items[]
│
└── If CustomerInvoice/CommissionInvoice/ReturnInvoice:
    └── LoadOrderBasedInvoiceItemsAsync()
        └── API: GET /api/Returns/OrderItemsByOrderId?orderCode={code}
        └── Fallback: orderId endpoints
```

### 4. API Endpoints Summary

| Invoice Type | API Endpoint | Parameter |
|--------------|--------------|-----------|
| Customer/Commission/Return | `/api/Returns/OrderItemsByOrderId` | `orderCode` (int) |
| Supplier | `/api/Invoices/GetSupplierInviceProductsByInvoicCode` | `invoiceCode` (int) |


---

## ✅ How to Use in Code

### Checking invoice type:
```csharp
var invoice = new InvoiceResponseDto { Type = "CustomerInvoice" };

if (invoice.ShouldLoadByOrderId)
{
    // Load items via OrderId
    var items = await _ordersService.GetOrderItemsByOrderIdAsync(invoice.OrderId.ToString());
}
else if (invoice.ShouldLoadByInvoiceCode)
{
    // Use embedded items (or fetch by InvoiceCode)
    var items = invoice.Items;
}
```

### Getting localized type name:
```csharp
var displayName = invoice.TypeDisplayName; // "فاتورة عميل"
```

---

## ❌ Previous Problems (Now Fixed)

| Problem | Root Cause | Solution |
|---------|------------|----------|
| Customer invoice items not loading | Mixed order/code logic | Strict OrderId loading |
| Commission invoice items not loading | Wrong identifier used | Always use OrderId |
| Return invoice items not loading | Fallback to InvoiceCode | Enforce OrderId rule |
| Inconsistent type checking | String comparisons | Type-safe enum |

---

## 🛡️ Bug Prevention

1. **Type Safety**: Using `InvoiceType` enum prevents string typos
2. **Single Decision Point**: `UsesOrderId()` is the ONLY method that decides loading strategy
3. **Clear Error Messages**: Arabic error messages for missing OrderId
4. **Fallback Handling**: If primary endpoint fails, tries alternate endpoint
5. **Null Safety**: Validates `OrderId` before attempting to load

---

## 📊 Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                 INVOICE DETAILS LOADING FLOW                    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │ Parse Type from │
                    │   API response  │
                    └────────┬────────┘
                             │
              ┌──────────────┴──────────────┐
              │                             │
              ▼                             ▼
    ┌─────────────────┐          ┌─────────────────┐
    │ SupplierInvoice │          │ Other Invoices  │
    │   (EXCEPTION)   │          │ Customer/Comm/  │
    │                 │          │     Return      │
    └────────┬────────┘          └────────┬────────┘
             │                            │
             ▼                            ▼
    ┌─────────────────┐          ┌─────────────────┐
    │ Use embedded    │          │ Validate that   │
    │ Items[] list    │          │ OrderId exists  │
    └────────┬────────┘          └────────┬────────┘
             │                            │
             │                   ┌────────┴────────┐
             │                   │                 │
             │                   ▼                 ▼
             │         ┌─────────────┐   ┌─────────────┐
             │         │   OrderId   │   │  No OrderId │
             │         │   present   │   │   → ERROR   │
             │         └──────┬──────┘   └─────────────┘
             │                │
             │                ▼
             │      ┌─────────────────┐
             │      │ Fetch items via │
             │      │   OrderId API   │
             │      └────────┬────────┘
             │               │
             ▼               ▼
    ┌─────────────────────────────────────┐
    │        Display Items in Grid        │
    └─────────────────────────────────────┘
```

---

## 🔒 MVVM Compliance

All changes maintain MVVM principles:
- **Model**: DTOs and Enums in separate files
- **ViewModel**: `InvoiceDetailsViewModel` handles all logic
- **View**: `InvoiceDetailsPage.xaml` binds to ViewModel properties
- **No Breaking Changes**: Existing API contracts unchanged

---

## 📝 Testing Checklist

- [ ] Customer Invoice: Shows order items via OrderId
- [ ] Commission Invoice: Shows order items via OrderId
- [ ] Return Invoice: Shows returned order items via OrderId
- [ ] Supplier Invoice: Shows embedded items (no OrderId needed)
- [ ] Missing OrderId: Shows appropriate Arabic error message
- [ ] Invalid type: Shows "نوع فاتورة غير معروف" error

---

*Document Version: 1.0*
*Created: 2026-01-22*
