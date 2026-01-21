using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels
{
    public class SalesReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public SalesReportViewModel()
        {
            _reportService = new ReportService(App.Api);

            FromDate = DateTime.Now.AddMonths(-1);
            ToDate = DateTime.Now;

            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
        }

        // ================= Filters =================
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

        // ================= Data =================
        private SalesReportDto _report;
        public SalesReportDto Report
        {
            get => _report;
            set => SetProperty(ref _report, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // ================= Commands =================
        public ICommand LoadReportCommand { get; }

        // ================= Logic =================
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
        
        public bool HasError => ErrorState != null && ErrorState.IsVisible;

        // ================= Logic =================
        private async Task LoadReportAsync()
        {
            try
            {
                ErrorState = Helpers.ReportErrorState.Empty;
                IsLoading = true;
                Report = null;

                var result = await _reportService.GetSalesReportAsync(FromDate, ToDate);
                if (result != null)
                {
                    Report = result;
                }
                else
                {
                    // Assuming null means empty or error, but GetSalesReportAsync returns SalesReportDto directly.
                    // If it throws exception, it's caught below.
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
