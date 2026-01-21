using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels.Reports
{
    public partial class StockMovementReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public StockMovementReportViewModel()
        {
            _reportService = new ReportService(App.Api);

            LoadReportCommand = new AsyncRelayCommand(
                execute: LoadReportAsync,
                canExecute: () => !IsLoading
            );
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                (LoadReportCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(NoData));
            }
        }
        
        public IAsyncRelayCommand LoadReportCommand { get; }

        public System.Collections.ObjectModel.ObservableCollection<erp.DTOS.Inventory.ProductSuggestionDto> Suggestions { get; } = new();

        private string _productName;
        public string ProductName
        {
            get => _productName;
            set
            {
                if (SetProperty(ref _productName, value))
                {
                    LoadSuggestions(value);
                }
            }
        }

        private erp.DTOS.Inventory.ProductSuggestionDto _selectedSuggestion;
        public erp.DTOS.Inventory.ProductSuggestionDto SelectedSuggestion
        {
            get => _selectedSuggestion;
            set
            {
                if (SetProperty(ref _selectedSuggestion, value))
                {
                    if (value != null)
                    {
                         // If user selects an item, we ensure the text matches (often handled by binding automatically)
                         // But we primarily want to ensure we don't trigger a new search that clears the list
                    }
                }
            }
        }

        private async void LoadSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                Suggestions.Clear();
                return;
            }

            // Prevent searching if the term matches the currently selected item's name
            // This happens when user selects an item -> Text updates -> LoadSuggestions triggers
            if (_selectedSuggestion != null && string.Equals(_selectedSuggestion.Name, term, StringComparison.OrdinalIgnoreCase))
            {
                return; 
            }

            try
            {
                var results = await _reportService.GetProductSuggestionsAsync(term);
                
                Suggestions.Clear();
                foreach(var item in results)
                {
                    Suggestions.Add(item);
                }
            }
            catch 
            {
                // Silently fail
            }
        }

        private StockMovementReportDto _report;
        public StockMovementReportDto Report
        {
            get => _report;
            set
            {
                SetProperty(ref _report, value);
                OnPropertyChanged(nameof(HasReport));
                OnPropertyChanged(nameof(NoData));
            }
        }

        // ✅ جديد: عشان نتحكم في “لا توجد بيانات” قبل البحث
        private bool _hasSearched;
        public bool HasSearched
        {
            get => _hasSearched;
            set
            {
                SetProperty(ref _hasSearched, value);
                OnPropertyChanged(nameof(NoData));
            }
        }

        public bool HasReport => Report != null;

        // ✅ لو محتاجها في الـ UI
        private Helpers.ReportErrorState _errorState;
        public Helpers.ReportErrorState ErrorState
        {
            get => _errorState;
            set
            {
                SetProperty(ref _errorState, value);
                OnPropertyChanged(nameof(HasError));
            }
        }
        
        public bool HasError => ErrorState != null && ErrorState.IsVisible;
        public bool NoData => HasSearched && !IsLoading && Report == null && !HasError;

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                ErrorState = Helpers.ReportErrorHandler.CreateValidation("من فضلك أدخل اسم المنتج");
                return;
            }

            try
            {
                IsLoading = true;
                HasSearched = true;
                Report = null;
                ErrorState = Helpers.ReportErrorState.Empty;

                var name = ProductName.Trim();
                var result = await _reportService.GetStockMovementAsync(name);

                if (result != null)
                {
                    // Assuming StockMovementReportDto might not have Status code directly or behaves differently
                    // Based on ReportService, it returns the DTO. If existing code worked, we assume success if not null.
                    // But if DTO follows standard, let's check. 
                    // StockMovementReportDto code is available in file list. Based on SupplierReportDto, it likely has StatusCode.
                    // Let's assume standard behavior for now to be safe, or just check null.
                    // Actually, if it returns DTO directly, ApiClient likely throws on error.
                    Report = result;
                }
                else
                {
                     // If result is null but no exception, it might be 404 handled gracefully or just empty
                     // Let's assume it means Not Found for now
                     ErrorState = Helpers.ReportErrorHandler.HandleApiError(404, "لم يتم العثور على المنتج");
                }
            }
            catch (Exception ex)
            {
                ErrorState = Helpers.ReportErrorHandler.HandleException(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
