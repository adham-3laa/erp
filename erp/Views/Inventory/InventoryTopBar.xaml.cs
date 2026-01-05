using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Inventory
{
    public partial class InventoryTopBar : UserControl
    {
        public InventoryTopBar()
        {
            InitializeComponent();
        }

        // ===== إضافة منتج =====
        public event RoutedEventHandler AddProductClicked;

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductClicked?.Invoke(sender, e);
        }

        // ===== جرد المخزون =====
        public event RoutedEventHandler InventoryCheckClicked;

        private void InventoryCheck_Click(object sender, RoutedEventArgs e)
        {
            InventoryCheckClicked?.Invoke(sender, e);
        }

        // ===== تحديث كمية منتجات (Stock In) =====
        public event RoutedEventHandler StockInClicked;

        private void StockIn_Click(object sender, RoutedEventArgs e)
        {
            StockInClicked?.Invoke(sender, e);
        }
    }
}
