using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS.Reports;
using erp.Services;
using System;
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
        }

        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
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

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasData => Report != null;
        public bool NoData => HasSearched && !IsLoading && Report == null;
        public bool ShowInitialState => !HasSearched && !IsLoading && Report == null;

        public IAsyncRelayCommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                HasError = true;
                ErrorMessage = "من فضلك أدخل اسم العميل";
                return;
            }

            try
            {
                HasError = false;
                ErrorMessage = null;
                IsLoading = true;
                HasSearched = true;
                Report = null;

                var result = await _reportService.GetCustomerReportAsync(CustomerName);
                if (result != null && result.StatusCode == 200)
                {
                    Report = result;
                }
                else if (result != null && result.StatusCode == 404)
                {
                    // No data found - NoData state will handle it
                    Report = null;
                }
                else
                {
                    HasError = true;
                    ErrorMessage = result?.Message ?? "تعذر جلب البيانات";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
