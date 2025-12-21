using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace erp.Views.Users
{
    public partial class AllUsersPage : UserControl
    {
        public AllUsersPage()
        {
            InitializeComponent();
            DataContext = new ViewModels.AllUsersViewModel();
        }
    }

    // محول Inverse Boolean محلي
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : false;
        }
    }
}