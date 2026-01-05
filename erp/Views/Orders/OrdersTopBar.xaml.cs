using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Orders
{
    public partial class OrdersTopBar : UserControl
    {
        public OrdersTopBar()
        {
            InitializeComponent();
        }

        // ✅ الأحداث المتبقية فقط
        public event RoutedEventHandler CreateOrderClicked;
        public event RoutedEventHandler ApprovedOrdersClicked;

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
            => CreateOrderClicked?.Invoke(this, e);

        private void ApprovedOrders_Click(object sender, RoutedEventArgs e)
            => ApprovedOrdersClicked?.Invoke(this, e);
    }
}
