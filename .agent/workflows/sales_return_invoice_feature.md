---
description: How to maintain and extend the Sales & Return Netting Invoice feature
---

# Sales & Return Netting Invoice Feature

This feature allows users to view a comprehensive financial statement (netting invoice) for a specific customer, comparing their sales invoices against supply/return invoices to show a net balance.

## components

### 1. Data Transfer Objects (DTOs)
- **NettingInvoiceResponseDto.cs**: Represents the API response structure.
  - Contains `SalesItems`, `SupplyItems`, `TotalSalesAmount`, `TotalSupplyAmount`, `NetBalance`, etc.

### 2. Service Layer
- **InvoiceService.cs**:
  - `GetNettingInvoiceAsync(partnerName, fromDate, toDate)`: Calls `GET /api/Invoices/netting-invoice`.
  - `GetPartnerNamesAutocompleteAsync(term)`: Helper for partner name autocomplete.

### 3. ViewModel
- **SalesReturnInvoiceViewModel.cs**:
  - Handles state: `SalesItems`, `SupplyItems`, `PartnerQuery`, `NetBalance`.
  - Implements autocomplete logic with debouncing (`System.Timers.Timer`).
  - Implements printing logic by generating an HTML file and opening it in the default browser/viewer.

### 4. View
- **SalesReturnInvoicePage.xaml**:
  - Simple, professional monochrome UI (Grays/Whites).
  - Uses `NiceDataGridStyle` for consistent look.
  - Displays summary cards for totals and net balance in neutral tones.

## Key Workflows

### Adding a new field to the report
1. Update `NettingInvoiceResponseDto.cs` to include the new field.
2. Update `SalesReturnInvoiceViewModel.cs` to map the new field.
3. Update `SalesReturnInvoicePage.xaml` to display the field (e.g., add a column to DataGrid).
4. Update the `BuildPrintHtml` method in the ViewModel to include the field in the printed report.

### Modifying the Print Layout
- The print layout is defined as a raw HTML string in `SalesReturnInvoiceViewModel.BuildPrintHtml`.
- Modify the CSS/HTML within that method to change the appearance of the printed invoice.

### Troubleshooting
- **API Errors**: Check `InvoiceService.cs` debug logs.
- **Autocomplete issues**: Ensure `GetPartnerNamesAutocompleteAsync` is returning results and the `SuggestionsPopup` in XAML is binding correctly.
