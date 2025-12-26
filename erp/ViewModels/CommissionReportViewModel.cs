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
        public bool NoData => !IsLoading && Items.Count == 0;


        private readonly ReportService _reportService;

        public CommissionReportViewModel()
        {
            _reportService = new ReportService(App.Api);
            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
        }

        private string _salesRepId;
        public string SalesRepId
        {
            get => _salesRepId;
            set => SetProperty(ref _salesRepId, value);
        }

        public ObservableCollection<CommissionReportItemDto> Items { get; }
            = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasData => Items.Count > 0;

        public ICommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(SalesRepId))
            {
                MessageBox.Show("من فضلك أدخل SalesRep ID");
                return;
            }

            try
            {
                IsLoading = true;
                Items.Clear();

                var response = await _reportService.GetCommissionsAsync(SalesRepId);

                if (response?.Value != null)
                {
                    foreach (var item in response.Value)
                        Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(NoData));
            }


        }
    }
}
