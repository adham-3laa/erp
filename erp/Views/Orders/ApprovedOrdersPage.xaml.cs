using System.Collections.Generic;
using System.Windows.Controls;

namespace EduGate.Views.Orders
{
    public partial class ApprovedOrdersPage : Page
    {
        public ApprovedOrdersPage()
        {
            InitializeComponent();
            LoadApprovedOrders();

            OrdersTopBarControl.CreateOrderClicked += (_, __) =>
                NavigationService.Navigate(new CreateOrderPage());

            OrdersTopBarControl.SalesRepOrdersClicked += (_, __) =>
                NavigationService.Navigate(new SalesRepOrdersPage());
        }


        private void LoadApprovedOrders()
        {
            // 🔹 الصفحة فاضية لحد ما نربط API
            OrdersDataGrid.ItemsSource = new List<object>();
        }

        private void ViewDetails_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // صفحة تفاصيل الطلب هنعملها بعدين
        }
    }
}
