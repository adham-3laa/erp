using System.Windows;
using System.Windows.Controls;

namespace EduGate.Views.Orders
{
    public partial class OrdersTopBar : UserControl
    {
        public OrdersTopBar()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler CreateOrderClicked;
        public event RoutedEventHandler SalesRepOrdersClicked;
        public event RoutedEventHandler ApprovedOrdersClicked;

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
            => CreateOrderClicked?.Invoke(this, e);

        private void SalesRepOrders_Click(object sender, RoutedEventArgs e)
            => SalesRepOrdersClicked?.Invoke(this, e);

        private void ApprovedOrders_Click(object sender, RoutedEventArgs e)
            => ApprovedOrdersClicked?.Invoke(this, e);
    }
}
