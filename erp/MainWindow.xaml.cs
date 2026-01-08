using erp.Views.Inventory;
using erp.Services;
using erp.ViewModels.Returns;
using erp.Views.Category;
using erp.Views.Dashboard;
using erp.Views.Expenses;
using erp.Views.Invoices;
using erp.Views.Payments;
using erp.Views.Returns;
using erp.Views.Users;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// ✅ Win32 Interop (بدون WindowsForms)
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace erp
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient;

        private void Logo_ImageFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(e.ErrorException?.Message ?? "Image failed");
        }

        public MainWindow()
        {
            InitializeComponent();

            // ✅ يفتح Full Screen تلقائيًا على مقاس الشاشة الحالية
            SourceInitialized += (_, __) => MoveToMouseMonitorWorkArea();
            Loaded += (_, __) => MaximizeToCurrentMonitorWorkArea();

            var httpClient = ApiClient.CreateHttpClient();
            _apiClient = new ApiClient(httpClient, null);

            erp.Services.NavigationService.Initialize(MainFrame);
            NavigateToDashboard();
        }

        // ====== Navigation Methods ======

        public void NavigateToDashboard()
        {
            MainFrame.Navigate(new DashboardPage());
            SelectNavItem("Dashboard");
        }

        public void NavigateToUsersPage()
        {
            MainFrame.Navigate(new AllUsersPage());
            SelectNavItem("Users");
        }

        public void NavigateToCurrentUser()
        {
            MainFrame.Navigate(new CurrentUserPage());
            SelectNavItem(null);
        }

        private void CurrentUserButton_Click(object sender, RoutedEventArgs e)
            => NavigateToCurrentUser();

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavListBox.SelectedItem is not ListBoxItem selectedItem)
                return;

            var tag = selectedItem.Tag as string;
            if (string.IsNullOrWhiteSpace(tag))
                return;

            switch (tag)
            {
                case "Dashboard":
                    NavigateToDashboard();
                    break;

                case "Users":
                    NavigateToUsersPage();
                    break;

                case "Inventory":
                    MainFrame.Navigate(new InventoryPage());
                    break;

                case "Invoices":
                    MainFrame.Navigate(new InvoicesListPage());
                    break;

                case "Expenses":
                    MainFrame.Navigate(new ExpensesListPage());
                    break;

                case "Items":
                    MainFrame.Navigate(new CategoryListPage());
                    break;

                case "Returns":
                    {
                        var returnsService = new ReturnsService(_apiClient);

                        var returnsVm = new ReturnsOrderItemsViewModel(returnsService);
                        var createReturnVm = new CreateReturnViewModel(returnsService);

                        MainFrame.Navigate(
                            new ReturnsOrderItemsView(returnsVm, createReturnVm)
                        );
                        break;
                    }

                case "Reports":
                    MainFrame.Navigate(new erp.Views.Reports.SalesReportPage());
                    break;

                case "Orders":
                    MainFrame.Navigate(new erp.Views.Orders.ApprovedOrdersPage());
                    SelectNavItem("Orders");
                    break;

                case "Suppliers":
                case "Auth":
                default:
                    ShowUnderDevelopment(tag);
                    break;
            }
        }

        private static void ShowUnderDevelopment(string tag)
        {
            string pageName = tag switch
            {
                "Dashboard" => "الداشبورد",
                "Orders" => "الطلبات",
                "Suppliers" => "الموردين",
                "Auth" => "المصادقة",
                _ => "الصفحة"
            };

            MessageBox.Show($"صفحة {pageName} قيد التطوير", "تطوير");
        }

        private void SelectNavItem(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                NavListBox.SelectedItem = null;
                return;
            }

            foreach (ListBoxItem item in NavListBox.Items)
            {
                if ((item.Tag as string) == tag)
                {
                    NavListBox.SelectedItem = item;
                    return;
                }
            }
        }

        // ====== Window Controls ======

        private void Sidebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Double click toggles maximize
            if (e.ClickCount == 2)
            {
                Max_Click(sender, e);
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                try { DragMove(); } catch { }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Min_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        // ====== ✅ Auto Full Screen on current monitor (Win32, no WinForms) ======

        private void MoveToMouseMonitorWorkArea()
        {
            // اختار الشاشة اللي عليها الماوس وقت تشغيل البرنامج
            if (!GetCursorPos(out POINT pt))
                return;

            IntPtr hMon = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
            if (hMon == IntPtr.Zero)
                return;

            RECT work = GetMonitorWorkArea(hMon);
            Rect dip = PixelRectToDip(work);

            // لازم Normal قبل تحديد Left/Top/Width/Height
            WindowState = WindowState.Normal;

            Left = dip.Left;
            Top = dip.Top;
            Width = dip.Width;
            Height = dip.Height;
        }

        private void MaximizeToCurrentMonitorWorkArea()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero)
                return;

            IntPtr hMon = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (hMon == IntPtr.Zero)
                return;

            RECT work = GetMonitorWorkArea(hMon);
            Rect dip = PixelRectToDip(work);

            WindowState = WindowState.Normal;

            Left = dip.Left;
            Top = dip.Top;

            // مهم جدًا مع WindowStyle=None + AllowsTransparency=True
            MaxWidth = dip.Width;
            MaxHeight = dip.Height;

            WindowState = WindowState.Maximized;
        }

        private Rect PixelRectToDip(RECT px)
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null)
            {
                // fallback
                return new Rect(px.Left, px.Top, px.Right - px.Left, px.Bottom - px.Top);
            }

            var m = source.CompositionTarget.TransformFromDevice;

            var tl = m.Transform(new Point(px.Left, px.Top));
            var br = m.Transform(new Point(px.Right, px.Bottom));

            return new Rect(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);
        }

        private static RECT GetMonitorWorkArea(IntPtr hMon)
        {
            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            if (!GetMonitorInfo(hMon, ref mi))
            {
                // fallback: empty rect
                return new RECT();
            }

            // rcWork = WorkArea بدون Taskbar
            return mi.rcWork;
        }

        // ====== Win32 ======

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    }
}
