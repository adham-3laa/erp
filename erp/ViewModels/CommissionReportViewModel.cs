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

        private string _salesRepId;
        public string SalesRepId
        {
            get => _salesRepId;
            set => SetProperty(ref _salesRepId, value);
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
        public bool HasData => !IsLoading && Items.Count > 0;

        // ✅ NoData مش هتظهر غير بعد أول بحث
        public bool NoData => HasSearched && !IsLoading && Items.Count == 0;

        public ICommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            // ✅ لو فاضي، ما نعتبرهاش بحث (علشان NoData ما يظهرش)
            if (string.IsNullOrWhiteSpace(SalesRepId))
            {
                MessageBox.Show("من فضلك أدخل SalesRep ID");
                return;
            }

            // ✅ أول ما يدوس تحميل = يعتبر بحث
            HasSearched = true;

            try
            {
                IsLoading = true;

                Items.Clear();
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(HasData));

                var response = await _reportService.GetCommissionsAsync(SalesRepId);

                if (response?.Value != null)
                {
                    foreach (var item in response.Value)
                        Items.Add(item);
                }

                // ✅ بعد إضافة الداتا
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(HasData));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);

                // ✅ في حالة خطأ: نخليها تظهر NoData بعد البحث لو مفيش Items
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(HasData));
            }
            finally
            {
                IsLoading = false;

                // ✅ تأكيد تحديث الحالات بعد التحميل
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(HasData));
            }
        }
    }
}
