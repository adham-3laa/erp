using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS.Reports;
using erp.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels
{
    public class SalesRepReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public SalesRepReportViewModel()
        {
            _reportService = new ReportService(App.Api);
            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
        }

        public System.Collections.ObjectModel.ObservableCollection<erp.DTOS.Orders.SalesRepSuggestionDto> Suggestions { get; } = new();

        private erp.DTOS.Orders.SalesRepSuggestionDto _selectedSuggestion;
        public erp.DTOS.Orders.SalesRepSuggestionDto SelectedSuggestion
        {
            get => _selectedSuggestion;
            set
            {
                if (SetProperty(ref _selectedSuggestion, value))
                {
                    if (value != null)
                    {
                        // Match logic similar to other VMS
                    }
                }
            }
        }

        private string _salesRepName;
        public string SalesRepName
        {
            get => _salesRepName;
            set
            {
                if (SetProperty(ref _salesRepName, value))
                {
                    LoadSuggestions(value);
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

            if (_selectedSuggestion != null && string.Equals(_selectedSuggestion.FullName, term, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                var results = await _reportService.GetSalesRepSuggestionsAsync(term);
                Suggestions.Clear();
                foreach (var item in results)
                {
                    Suggestions.Add(item);
                }
            }
            catch
            {
                // Ignore
            }
        }

        private SalesRepReportDto _report;
        public SalesRepReportDto Report
        {
            get => _report;
            set
            {
                SetProperty(ref _report, value);
                OnPropertyChanged(nameof(HasData));
                OnPropertyChanged(nameof(HasUnpaidCommissions));
                OnPropertyChanged(nameof(NoUnpaidCommissions));
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
        public bool HasUnpaidCommissions => Report?.UnpaidCommissions?.Count > 0;
        public bool NoUnpaidCommissions => Report != null && (Report.UnpaidCommissions == null || Report.UnpaidCommissions.Count == 0);

        public IAsyncRelayCommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(SalesRepName))
            {
                ErrorState = Helpers.ReportErrorHandler.CreateValidation("من فضلك أدخل اسم مندوب المبيعات");
                return;
            }

            try
            {
                ErrorState = Helpers.ReportErrorState.Empty;
                IsLoading = true;
                HasSearched = true;
                Report = null;

                var result = await _reportService.GetSalesRepReportAsync(SalesRepName);
                if (result != null)
                {
                   if(result.StatusCode == 200)
                    {
                        Report = result;
                    }
                    else if (result.StatusCode == 404)
                    {
                        Report = null; // NoData state
                    }
                    else
                    {
                        ErrorState = Helpers.ReportErrorHandler.HandleApiError(result.StatusCode, result.Message);
                    }
                }
                else
                {
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
