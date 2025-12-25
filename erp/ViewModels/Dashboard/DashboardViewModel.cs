using erp.Commands;
using erp.DTOS.Dashboard;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace erp.ViewModels.Dashboard
{
    public sealed class DashboardViewModel : BaseViewModel
    {
        private readonly DashboardService _dashboardService;
        private CancellationTokenSource? _cts;

        // ✅ ctor فاضي عشان XAML
        public DashboardViewModel() : this(App.Dashboard) { }

        public DashboardViewModel(DashboardService dashboardService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        }

        public AsyncRelayCommand RefreshCommand { get; }

        public ObservableCollection<LowStockProductDto> LowStockProducts { get; } = new();

        private int _lowStockCount;
        public int LowStockCount
        {
            get => _lowStockCount;
            private set => Set(ref _lowStockCount, value);
        }

        private decimal _totalSalesToday;
        public decimal TotalSalesToday { get => _totalSalesToday; set => Set(ref _totalSalesToday, value); }

        private decimal _totalProfitToday;
        public decimal TotalProfitToday { get => _totalProfitToday; set => Set(ref _totalProfitToday, value); }

        private int _pendingOrdersCount;
        public int PendingOrdersCount { get => _pendingOrdersCount; set => Set(ref _pendingOrdersCount, value); }

        private int _approvedOrdersCountToday;
        public int ApprovedOrdersCountToday { get => _approvedOrdersCountToday; set => Set(ref _approvedOrdersCountToday, value); }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (Set(ref _isBusy, value))
                    RefreshCommand.RaiseCanExecuteChanged();
            }
        }

        private string? _error;
        public string? Error { get => _error; private set => Set(ref _error, value); }

        public async Task RefreshAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                Error = null;
                IsBusy = true;

                var stats = await _dashboardService.GetStatsAsync(_cts.Token);

                TotalSalesToday = stats.TotalSalesToday;
                TotalProfitToday = stats.TotalProfitToday;
                PendingOrdersCount = stats.PendingOrdersCount;
                ApprovedOrdersCountToday = stats.ApprovedOrdersCountToday;

                LowStockProducts.Clear();
                foreach (var p in stats.LowStockProducts)
                    LowStockProducts.Add(p);

                LowStockCount = LowStockProducts.Count;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
