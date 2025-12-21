using System.Windows;
using System.Windows.Controls;

namespace EduGate.Views.Inventory
{
    public partial class InventoryTopBar : UserControl
    {
        public InventoryTopBar()
        {
            InitializeComponent(); // خليك هنا فقط، من غير أي throw
        }

        // الحدث اللي هيتربط بالـ InventoryPage
        public event RoutedEventHandler AddProductClicked;

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductClicked?.Invoke(sender, e);
        }

        private void ShowProducts_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as erp.MainWindow)?.MainFrame.Navigate(new InventoryPage());
        }
    }
}
