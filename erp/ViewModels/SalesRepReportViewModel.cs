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

        private string _salesRepName;
        public string SalesRepName
        {
            get => _salesRepName;
            set => SetProperty(ref _salesRepName, value);
        }

        private SalesRepReportDto _report;
        public SalesRepReportDto Report
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
            }
        }

        public bool HasData => Report != null;
        public bool NoData => HasSearched && !IsLoading && Report == null;

        public IAsyncRelayCommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(SalesRepName))
            {
                MessageBox.Show("من فضلك أدخل اسم مندوب المبيعات", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                HasSearched = true;
                Report = null;

                var result = await _reportService.GetSalesRepReportAsync(SalesRepName);
                if (result != null && result.StatusCode == 200)
                {
                    Report = result;
                }
                else
                {
                    MessageBox.Show(result?.Message ?? "تعذر جلب البيانات", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
