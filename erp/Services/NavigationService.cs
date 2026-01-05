using erp.DTOS;
using erp.ViewModels;
// Reports
using erp.Views.Reports;
// Users
using erp.Views.Users;
using System.Windows.Controls;

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
        //======================InvoicesForUsers==============
        public static void NavigateToUserInvoices(UserDto user)
        {
            _mainFrame?.Navigate(new UserInvoicesPage(user));
        }

        // Overload جديد لقبول WrappedUserDto
        public static void NavigateToUserInvoices(WrappedUserDto wrappedUser)
        {
            // تحويل WrappedUserDto إلى UserDto
            var userDto = new UserDto
            {
                Id = wrappedUser.Id,
                Fullname = wrappedUser.Fullname,
                Username = wrappedUser.Username,
                Email = wrappedUser.Email,
                SalesRepId = wrappedUser.SalesRepId,
                Phonenumber = wrappedUser.Phonenumber,
                UserType = wrappedUser.UserType,
                IsActive = wrappedUser.IsActive,
                ImagePath = wrappedUser.ImagePath,
                DateOfCreation = wrappedUser.DateOfCreation,
                FarmsCount = wrappedUser.FarmsCount
            };

            _mainFrame?.Navigate(new UserInvoicesPage(userDto));
        }

        // Overload آخر لإرسال ID فقط
        public static void NavigateToUserInvoices(string userId)
        {
            // يمكنك إنشاء UserDto مؤقت أو تعديل الصفحة لتقبل ID فقط
            var userDto = new UserDto
            {
                Id = userId,
                Fullname = "مستخدم",
                // باقي الخصائص يمكن تركها null أو قيم افتراضية
            };

            _mainFrame?.Navigate(new UserInvoicesPage(userDto));
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
        public static void NavigateToUserProfile(string userId)
        {
            _mainFrame.Navigate(new CurrentUserPage(userId));
        }


    }
}
