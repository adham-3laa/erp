# Enhanced Returns Module Documentation

## Overview
The Returns module has been overhauled to provide a streamlined, step-by-step experience for both Customer Returns (Sales Return) and Supplier Returns (Purchase Return). The new design features a modern UI with clear progress indicators, validation, and improved error handling using Arabic messages.

## Key Features

### 1. Unified Entry Point
- Accessible via the **"Returns"** (إدارة المرتجعات) item in the main sidebar.
- Automatically loads the new `EnhancedCreateReturnView`.

### 2. Dual Return Modes
- **Return from Customer (مرتجع عميل):**
  - **Step 1:** Enter Order ID (Search & Validate against strict order data).
  - **Step 2:** Select items from the order to return.
  - **Step 3:** Specify return quantities and reasons (Quantity validated against sold amount).
  - **Step 4:** Review and Confirm.
- **Return to Supplier (مرتجع مورد):**
  - **Step 1:** Select Supplier (searchable auto-complete).
  - **Step 2:** Add items manually (Product search with auto-complete, quantity, reason).
  - **Step 3:** Review item list.
  - **Step 4:** Confirm return.

### 3. Technical Enhancements
- **Async Operations:** UI remains responsive during API calls using `AsyncRelayCommand`.
- **Validation:** Strict validation loops ensuring data integrity before submission.
- **Error Handling:** Centralized `ErrorHandlingService` provides user-friendly Arabic messages.
- **Architecture:** Clean MVVM pattern with `EnhancedCreateReturnViewModel` and reusable commands.

## Code Changes & Integration
- **MainWindow:** Updated navigation logic to route "Returns" requests to `EnhancedCreateReturnView`.
- **Commands:** Centralized `RelayCommand` and `AsyncRelayCommand` in `erp.Commands` to resolve definition conflicts.
- **Styles:** Fixed duplicate style definitions in XAML resources.
- **Converters:** Integrated `InverseBoolToVisibilityConverter` for better step visibility control.

## Developer Notes
- **ViewModel:** `erp.ViewModels.Returns.EnhancedCreateReturnViewModel`
- **View:** `erp.Views.EnhancedCreateReturnView`
- **Dependencies:** `ReturnsService`, `InventoryService`, `UserService`.

This module is now fully integrated and replaces the legacy returns workflow.
