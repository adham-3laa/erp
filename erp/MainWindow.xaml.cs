using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EduGate.Views.Accountants;

namespace EduGate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NavListBox.SelectionChanged += NavListBox_SelectionChanged;

            // الصفحة الافتراضية: المحاسبين (index = 1)
            NavListBox.SelectedIndex = 1;
        }

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = NavListBox.SelectedIndex;
            if (index < 0) return;

            NavigateToIndex(index);
        }

        private void NavigateToIndex(int index)
        {
            AccountantsTopBarControl.Visibility = Visibility.Collapsed;
            MainFrame.Content = null;

            switch (index)
            {
                case 0: break; // المستخدمين

                case 1:
                    AccountantsTopBarControl.Visibility = Visibility.Visible;
                    MainFrame.Navigate(new AllAccountantsPage());
                    break;

                case 2: break; // المخزون
                case 3: break; // الفواتير
                case 4: break; // الطلبات
                case 5: break; // المصروفات
                case 6: break; // الأصناف
                case 7: break; // الموردين
                case 8: break; // المصادقة
            }
        }

        // ✅ Window Buttons
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Min_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized)
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        // ✅ Drag window from sidebar (Double click toggles maximize)
        private void Sidebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Max_Click(sender, e);
                return;
            }

            try { DragMove(); } catch { }
        }
    }
}
