using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace erp.ViewModels.Reports
{
    public class StockMovementReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public StockMovementReportViewModel()
        {
            _reportService = new ReportService(App.Api);
            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
        }

        private string _productId;
        public string ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        private StockMovementReportDto _report;
        public StockMovementReportDto Report
        {
            get => _report;
            set
            {
                SetProperty(ref _report, value);
                OnPropertyChanged(nameof(HasReport));
            }
        }

        public bool HasReport => Report != null;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(ProductId))
            {
                MessageBox.Show("من فضلك أدخل Product ID");
                return;
            }

            try
            {
                IsLoading = true;
                Report = await _reportService.GetStockMovementAsync(ProductId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
