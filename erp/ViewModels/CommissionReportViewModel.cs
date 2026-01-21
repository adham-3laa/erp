using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels.Reports
{
    public class CommissionReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public CommissionReportViewModel()
        {
            _reportService = new ReportService(App.Api);
            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
        }

        // ✅ جديد: علشان نخفي NoData قبل أول بحث
        private bool _hasSearched;
        public bool HasSearched
        {
            get => _hasSearched;
            set
            {
                SetProperty(ref _hasSearched, value);
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(HasData));
            }
        }

        public ObservableCollection<erp.DTOS.Orders.SalesRepSuggestionDto> Suggestions { get; } = new();

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
                        // Ensure text matches selected suggestion (usually automatic via binding)
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
                // Ignore errors for autocomplete
            }
        }

        public ObservableCollection<CommissionReportItemDto> Items { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(HasData));
            }
        }

        // ✅ HasData محسوبة صح + تتأثر بـ IsLoading
        // ✅ جديد: لإدارة الأخطاء
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

        // ✅ HasData محسوبة صح
        public bool HasData => !IsLoading && Items.Count > 0 && !HasError;

        // ✅ NoData مش هتظهر غير بعد أول بحث، ولو مفيش أخطاء
        public bool NoData => HasSearched && !IsLoading && Items.Count == 0 && !HasError;

        public ICommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(SalesRepName))
            {
                ErrorState = Helpers.ReportErrorHandler.CreateValidation("من فضلك أدخل اسم مندوب المبيعات");
                return;
            }

            try
            {
                IsLoading = true;
                HasSearched = true;
                Items.Clear();
                ErrorState = Helpers.ReportErrorState.Empty;

                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(HasData));

                var response = await _reportService.GetCommissionsAsync(SalesRepName);

                if (response?.Value != null)
                {
                    foreach (var item in response.Value)
                        Items.Add(item);

                    if(Items.Count == 0)
                    {
                         // Optional: You could treat empty list as "No Data" state, handled by NoData property
                    }
                }
                else
                {
                    // If API returns null without exception, treated as empty or logic error can be added here
                }
            }
            catch (Exception ex)
            {
                ErrorState = Helpers.ReportErrorHandler.HandleException(ex);
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(HasData));
            }
        }
    }
}
