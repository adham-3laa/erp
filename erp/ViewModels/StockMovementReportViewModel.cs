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

        private string _productName;
        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
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
        public bool NoData => HasSearched && !IsLoading && Report == null;

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

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                MessageBox.Show("من فضلك أدخل اسم المنتج");
                return;
            }

            HasSearched = true;
            Report = null; // ✅ امسح التقرير القديم قبل تحميل الجديد

            try
            {
                IsLoading = true;

                // لو الـ API حساس للحروف: خليه Trim
                var name = ProductName.Trim();

                // ✅ انت بتستخدم productName مع الـ API
                Report = await _reportService.GetStockMovementAsync(name);

                // لو رجع null
                if (Report == null)
                    MessageBox.Show("لا توجد بيانات لهذا المنتج");
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
