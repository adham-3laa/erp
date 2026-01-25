using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.DTOS.Reports;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace erp.ViewModels
{
    public class DualRoleReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public DualRoleReportViewModel()
        {
            _reportService = new ReportService(App.Api);

            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
            
            // Load autocomplete data
            LoadSuppliersAsync();
        }

        // ================= Autocomplete Logic =================
        private ObservableCollection<SupplierDto> _allSuppliers = new();
        private ObservableCollection<string> _suggestions = new();
        public ObservableCollection<string> Suggestions
        {
            get => _suggestions;
            set => SetProperty(ref _suggestions, value);
        }

        private bool _isSuppliersLoading;
        public bool IsSuppliersLoading
        {
            get => _isSuppliersLoading;
            set => SetProperty(ref _isSuppliersLoading, value);
        }

        private string _searchName;
        public string SearchName
        {
            get => _searchName;
            set
            {
                if (SetProperty(ref _searchName, value))
                {
                    UpdateSuggestions(value);
                    // If user selects exact match, auto-load? 
                    // Let's stick to manual load or exact match trigger logic similar to SupplierReport
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
            foreach (var item in filtered)
            {
                Suggestions.Add(item);
            }
        }

        private async void LoadSuppliersAsync()
        {
            try
            {
                IsSuppliersLoading = true;
                var result = await _reportService.GetSuppliersAsync();
                if (result?.Value != null)
                {
                    _allSuppliers = new ObservableCollection<SupplierDto>(result.Value);
                }
            }
            catch (Exception)
            {
                // Silent fail for autocomplete
            }
            finally
            {
                IsSuppliersLoading = false;
            }
        }

        // ================= Report Data =================
        private DualRoleReportDto _report;
        public DualRoleReportDto Report
        {
            get => _report;
            set
            {
                SetProperty(ref _report, value);
                OnPropertyChanged(nameof(HasData));
                OnPropertyChanged(nameof(NoData));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(IsNotLoading));
                OnPropertyChanged(nameof(NoData));
            }
        }

        public bool IsNotLoading => !IsLoading;

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

        // ================= Error Handling =================
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

        public bool HasData => Report != null && !HasError;
        public bool NoData => HasSearched && !IsLoading && Report == null && !HasError;
        public bool HasError => ErrorState != null && ErrorState.IsVisible;
        public bool ShowInitialState => !HasSearched && !IsLoading;

        // ================= Commands =================
        public IAsyncRelayCommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchName))
            {
                ErrorState = Helpers.ReportErrorHandler.CreateValidation("يرجى إدخال الاسم للبحث");
                return;
            }

            try
            {
                IsLoading = true;
                HasSearched = true;
                Report = null;
                ErrorState = Helpers.ReportErrorState.Empty;

                var result = await _reportService.GetDualRoleReportAsync(SearchName);
                
                if (result != null)
                {
                    if (result.StatusCode == 200)
                    {
                        Report = result;
                    }
                    else
                    {
                        ErrorState = Helpers.ReportErrorHandler.HandleApiError(result.StatusCode, result.Message);
                    }
                }
                else
                {
                     ErrorState = Helpers.ReportErrorHandler.HandleException(new Exception("لم يتم استلام أي رد من الخادم"));
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
