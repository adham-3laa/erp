using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS.Reports;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace erp.ViewModels
{
    public class FullSalesReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public FullSalesReportViewModel()
        {
            _reportService = new ReportService(App.Api);

            // Default to last 30 days
            FromDate = DateTime.Now.AddDays(-30);
            ToDate = DateTime.Now;

            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
        }

        // ================= Filter Properties =================
        private DateTime _fromDate;
        public DateTime FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        private DateTime _toDate;
        public DateTime ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        // ================= Report Data =================
        private FullSalesReportDto _report;
        public FullSalesReportDto Report
        {
            get => _report;
            set
            {
                SetProperty(ref _report, value);
                OnPropertyChanged(nameof(HasData));
                OnPropertyChanged(nameof(NoData));
            }
        }

        // ================= Loading State =================
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

        // ================= State Flags =================
        public bool HasData => Report != null && !HasError;
        public bool NoData => HasSearched && !IsLoading && Report == null && !HasError;
        public bool HasError => ErrorState != null && ErrorState.IsVisible;
        public bool ShowInitialState => !HasSearched && !IsLoading;

        // ================= Computed Properties for Charts =================
        public decimal MaxDailyRevenue => Report?.DailyTrend?.Any() == true 
            ? Report.DailyTrend.Max(d => d.Revenue) 
            : 0;

        public decimal MaxDailyProfit => Report?.DailyTrend?.Any() == true 
            ? Report.DailyTrend.Max(d => d.Profit) 
            : 0;

        // ================= Commands =================
        public IAsyncRelayCommand LoadReportCommand { get; }

        // ================= Logic =================
        private async Task LoadReportAsync()
        {
            // Validation
            if (FromDate > ToDate)
            {
                ErrorState = Helpers.ReportErrorHandler.CreateValidation("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");
                return;
            }

            try
            {
                IsLoading = true;
                HasSearched = true;
                Report = null;
                ErrorState = Helpers.ReportErrorState.Empty;

                var result = await _reportService.GetFullSalesReportAsync(FromDate, ToDate);
                
                if (result != null)
                {
                    if (result.StatusCode == 200)
                    {
                        Report = result;
                        
                        // Notify chart-related properties
                        OnPropertyChanged(nameof(MaxDailyRevenue));
                        OnPropertyChanged(nameof(MaxDailyProfit));
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
