using erp;
using erp.DTOS.Orders;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Orders
{
    public partial class ApprovedOrdersPage : Page
    {
        private readonly OrdersService _ordersService;
        private List<OrderDto> _orders = new List<OrderDto>();
        private bool _isLoading = false;

        public ApprovedOrdersPage()
        {
            InitializeComponent();

            // ✅ ApiClient جاهز وفيه Token
            _ordersService = new OrdersService(App.Api);

            OrdersTopBarControl.CreateOrderClicked += (_, __) =>
                NavigationService?.Navigate(new CreateOrderPage());

            Loaded += LoadApprovedOrders;
        }

        #region Load Orders

        private async void LoadApprovedOrders(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            try
            {
                SetLoadingState(true);
                HideError();

                _orders = await _ordersService.GetApprovedOrdersAsync();

                if (_orders == null || _orders.Count == 0)
                {
                    ShowEmptyState("لا توجد طلبات معتمدة", "سيتم عرض الطلبات هنا بعد اعتمادها");
                    OrdersCountText.Text = "لا توجد طلبات";
                }
                else
                {
                    HideEmptyState();
                    OrdersDataGrid.ItemsSource = _orders;
                    OrdersCountText.Text = $"إجمالي {_orders.Count} طلب معتمد";
                }
            }
            catch (Exception ex)
            {
                ShowError($"حدث خطأ أثناء تحميل الطلبات: {ex.Message}");
                ShowEmptyState("فشل في تحميل البيانات", "تحقق من الاتصال بالإنترنت وحاول مرة أخرى");
                OrdersCountText.Text = "خطأ في التحميل";
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        #endregion

        #region Refresh

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadApprovedOrders(sender, e);
        }

        #endregion

        #region UI State Methods

        private void SetLoadingState(bool isLoading)
        {
            _isLoading = isLoading;
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBanner.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorBanner.Visibility = Visibility.Collapsed;
        }

        private void CloseError_Click(object sender, RoutedEventArgs e)
        {
            HideError();
        }

        private void ShowEmptyState(string title, string subtitle)
        {
            EmptyStateTitle.Text = title;
            EmptyStateSubtitle.Text = subtitle;
            EmptyState.Visibility = Visibility.Visible;
            TableCard.Visibility = Visibility.Collapsed;
        }

        private void HideEmptyState()
        {
            EmptyState.Visibility = Visibility.Collapsed;
            TableCard.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
