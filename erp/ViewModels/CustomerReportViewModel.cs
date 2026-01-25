using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS.Reports;
using erp.Services;
using System;
using System.Threading.Tasks;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels.Reports
{
    public class CustomerReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public CustomerReportViewModel()
        {
            _reportService = new ReportService(App.Api);
            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
            LoadSuppliersAsync();
        }

        public System.Collections.ObjectModel.ObservableCollection<string> Suggestions { get; } = new();

        private System.Collections.ObjectModel.ObservableCollection<erp.DTOS.SupplierDto> _allSuppliers = new();

        private bool _isSuppliersLoading;
        public bool IsSuppliersLoading
        {
            get => _isSuppliersLoading;
            set => SetProperty(ref _isSuppliersLoading, value);
        }

        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (SetProperty(ref _customerName, value))
                {
                    UpdateSuggestions(value);
                    if (_allSuppliers.Any(s => s.Name != null && s.Name.Equals(value, StringComparison.OrdinalIgnoreCase)))
                    {
                        LoadReportCommand.Execute(null);
                    }
                }
            }
        }

        private void UpdateSuggestions(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                Suggestions.Clear();
                return;
            }

            var filtered = _allSuppliers
                .Where(s => s.Name != null && s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Name)
                .Take(10)
                .ToList();

            Suggestions.Clear();
            foreach (var suggestion in filtered)
            {
                Suggestions.Add(suggestion);
            }
        }

        private async void LoadSuppliersAsync()
        {
            try
            {
                IsSuppliersLoading = true;
                var result = await _reportService.GetSuppliersAsync();
                if (result != null && result.StatusCode == 200 && result.Value != null)
                {
                    _allSuppliers = new System.Collections.ObjectModel.ObservableCollection<erp.DTOS.SupplierDto>(result.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load suppliers: {ex.Message}");
            }
            finally
            {
                IsSuppliersLoading = false;
            }
        }

        private CustomerReportDto _report;
        public CustomerReportDto Report
        {
            get => _report;
            set
            {
                SetProperty(ref _report, value);
                OnPropertyChanged(nameof(HasData));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(ShowInitialState));
            }
        }

        private bool _hasSearched;
        public bool HasSearched
        {
            get => _hasSearched;
            set
            {
                SetProperty(ref _hasSearched, value);
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(ShowInitialState));
            }
        }

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

        public bool HasData => Report != null;
        public bool NoData => HasSearched && !IsLoading && Report == null && !HasError;
        public bool ShowInitialState => !HasSearched && !IsLoading && Report == null;

        public IAsyncRelayCommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                ErrorState = Helpers.ReportErrorHandler.CreateValidation("من فضلك أدخل اسم العميل");
                return;
            }

            try
            {
                ErrorState = Helpers.ReportErrorState.Empty;
                IsLoading = true;
                HasSearched = true;
                Report = null;

                var result = await _reportService.GetCustomerReportAsync(CustomerName);
                if (result != null)
                {
                    if (result.StatusCode == 200)
                    {
                        Report = result;
                    }
                    else if (result.StatusCode == 404)
                    {
                         // 404 handled effectively by "NoData" state if that's preferred, 
                         // OR we can show a specific "Customer Not Found" error.
                         // Current logic sets Report=null, so NoData becomes true.
                         // Let's stick to NoData for "Not Found" searches, but if it is an API error, use Error.
                         // If API returns 404 for "Customer does not exist", NoData is better UX than Red Error.
                         // But if it's "Endpoint not found", that's an error.
                         // Assuming 404 = Customer not found here.
                         Report = null; 
                    }
                    else
                    {
                        ErrorState = Helpers.ReportErrorHandler.HandleApiError(result.StatusCode, result.Message);
                    }
                }
                else
                {
                     // Null result usually means something went wrong if not 404
                     ErrorState = Helpers.ReportErrorHandler.HandleException(new Exception("لم يتم استلام رد من الخادم"));
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
