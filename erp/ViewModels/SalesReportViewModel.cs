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
        private async Task LoadReportAsync()
        {
            try
            {
                IsLoading = true;
                Report = await _reportService.GetSalesReportAsync(FromDate, ToDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
