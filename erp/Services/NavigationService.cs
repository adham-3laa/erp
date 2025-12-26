using System.Windows.Controls;

// Users
using erp.Views.Users;

// Reports
using erp.Views.Reports;

namespace erp.Services
{
    public static class NavigationService
    {
        private static Frame _mainFrame;

        // ===================== INIT =====================
        public static void Initialize(Frame mainFrame)
        {
            _mainFrame = mainFrame;
        }

        // ===================== USERS =====================
        public static void NavigateToUsers()
        {
            _mainFrame?.Navigate(new AllUsersPage());
        }

        public static void NavigateToCreateUser()
        {
            _mainFrame?.Navigate(new CreateUserPage());
        }

        public static void NavigateToUpdateUser(string userId)
        {
            _mainFrame?.Navigate(new UpdateUserPage(userId));
        }

        public static void NavigateToCurrentUser()
        {
            _mainFrame?.Navigate(new CurrentUserPage());
        }

        // ===================== REPORTS =====================
        public static void NavigateToSalesReport()
        {
            _mainFrame?.Navigate(new SalesReportPage());
        }

        public static void NavigateToStockMovementReport()
        {
            _mainFrame?.Navigate(new StockMovementReportPage());
        }

        // ===================== BACK =====================
        public static void NavigateBack()
        {
            if (_mainFrame?.CanGoBack == true)
                _mainFrame.GoBack();
        }
        public static void NavigateToCommissionReport()
        {
            _mainFrame?.Navigate(new erp.Views.Reports.CommissionReportPage());
        }

    }
}
