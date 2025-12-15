using System.Windows;
using System.Windows.Controls;
using EduGate.Views.Accountants;

namespace EduGate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NavListBox.SelectionChanged += NavListBox_SelectionChanged;
        }

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavListBox.SelectedItem == null) return;

            var selected = ((ListBoxItem)NavListBox.SelectedItem).Content.ToString();

            switch (selected)
            {
                case "🧮 المحاسبين":
                    // إظهار TopBar للمحاسبين
                    AccountantsTopBarControl.Visibility = Visibility.Visible;

                    // تنقل إلى الصفحة الرئيسية للمحاسبين (كل المحاسبين)
                    MainFrame.Navigate(new AllAccountantsPage());
                    break;

                default:
                    // اخفاء TopBar لأي صفحة غير المحاسبين
                    AccountantsTopBarControl.Visibility = Visibility.Collapsed;
                    MainFrame.Content = null;
                    break;
            }

            // بعد الاختيار، ازالة التحديد عشان المستخدم يقدر يضغط مرة تانية
            NavListBox.SelectedItem = null;
        }
    }
}
