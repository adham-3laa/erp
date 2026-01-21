using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.DTOS.Reports;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace erp.ViewModels
{
    public class SupplierReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public SupplierReportViewModel()
        {
            _reportService = new ReportService(App.Api);
            LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);
            PrintReportCommand = new RelayCommand<object>(PrintReport, CanPrintReport);
            
            // Load suppliers for autocomplete
            LoadSuppliersAsync();
        }

        private ObservableCollection<SupplierDto> _allSuppliers = new();
        private ObservableCollection<string> _supplierSuggestions = new();
        public ObservableCollection<string> SupplierSuggestions
        {
            get => _supplierSuggestions;
            set => SetProperty(ref _supplierSuggestions, value);
        }

        private bool _isSuppliersLoading;
        public bool IsSuppliersLoading
        {
            get => _isSuppliersLoading;
            set => SetProperty(ref _isSuppliersLoading, value);
        }

        private string _supplierName;
        public string SupplierName
        {
            get => _supplierName;
            set
            {
                if (SetProperty(ref _supplierName, value))
                {
                    UpdateSuggestions(value);
                    // Seamless integration: If user selects an exact name from our list, auto-trigger load
                    if (_allSuppliers.Any(s => s.Name != null && s.Name.Equals(value, StringComparison.OrdinalIgnoreCase)))
                    {
                        LoadReportCommand.Execute(null);
                    }
                }
            }
        }

        private void UpdateSuggestions(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                SupplierSuggestions.Clear();
                return;
            }

            var filtered = _allSuppliers
                .Where(s => s.Name != null && s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Name)
                .Take(10)
                .ToList();

            SupplierSuggestions.Clear();
            foreach (var suggestion in filtered)
            {
                SupplierSuggestions.Add(suggestion);
            }
        }

        private SupplierReportDto _report;
        public SupplierReportDto Report
        {
            get => _report;
            set
            {
                SetProperty(ref _report, value);
                OnPropertyChanged(nameof(HasData));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(NoData));
            }
        }

        private bool _hasSearched;
        public bool HasSearched
        {
            get => _hasSearched;
            set
            {
                SetProperty(ref _hasSearched, value);
                OnPropertyChanged(nameof(NoData));
                OnPropertyChanged(nameof(ShowInitialState));
            }
        }

        private Helpers.ReportErrorState _errorState;
        public Helpers.ReportErrorState ErrorState
        {
            get => _errorState;
            set
            {
                SetProperty(ref _errorState, value);
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasData => Report != null;
        public bool NoData => HasSearched && !IsLoading && Report == null && !HasError;
        public bool HasError => ErrorState != null && ErrorState.IsVisible;
        public bool ShowInitialState => !HasSearched && !IsLoading;

        public IAsyncRelayCommand LoadReportCommand { get; }
        public IRelayCommand<object> PrintReportCommand { get; }

        private async Task LoadSuppliersAsync()
        {
            try
            {
                IsSuppliersLoading = true;
                var result = await _reportService.GetSuppliersAsync();
                if (result != null && result.StatusCode == 200 && result.Value != null)
                {
                    _allSuppliers = new ObservableCollection<SupplierDto>(result.Value);
                }
            }
            catch (Exception ex)
            {
                // Silently fail - autocomplete is not critical
                System.Diagnostics.Debug.WriteLine($"Failed to load suppliers: {ex.Message}");
            }
            finally
            {
                IsSuppliersLoading = false;
            }
        }

        private async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(SupplierName))
            {
                ErrorState = Helpers.ReportErrorHandler.CreateValidation("من فضلك أدخل اسم المورد");
                return;
            }

            try
            {
                IsLoading = true;
                HasSearched = true;
                Report = null;
                ErrorState = Helpers.ReportErrorState.Empty; // Clear errors

                var result = await _reportService.GetSupplierReportAsync(SupplierName);
                if (result != null)
                {
                    if (result.StatusCode == 200)
                    {
                        Report = result;
                    }
                    else
                    {
                        ErrorState = Helpers.ReportErrorHandler.HandleApiError(result.StatusCode, result.Message);
                    }
                }
                else
                {
                    ErrorState = Helpers.ReportErrorHandler.HandleException(new Exception("لم يتم استلام أي رد من الخادم"));
                }
            }
            catch (Exception ex)
            {
                ErrorState = Helpers.ReportErrorHandler.HandleException(ex);
            }
            finally
            {
                IsLoading = false;
                PrintReportCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanPrintReport(object parameter)
        {
            return Report != null;
        }

        private void PrintReport(object parameter)
        {
            try
            {
                if (parameter is not Page page)
                    return;

                var printArea = page.FindName("PrintArea") as FrameworkElement;
                if (printArea == null) return;

                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    try
                    {
                        // 1. تحديد عرض "الهدف" للطباعة (عرض قياسي يضمن ظهور كل الأعمدة بوضوح)
                        double targetWidth = 1000; // بكسل
                        double printableWidth = printDialog.PrintableAreaWidth;
                        double scale = printableWidth / (targetWidth + 40);

                        // 2. إجبار العنصر على التنسيق بهذا العرض (هذا يحل مشكلة اختفاء الخانات)
                        printArea.UpdateLayout();
                        printArea.Measure(new Size(targetWidth, double.PositiveInfinity));
                        Size contentSize = printArea.DesiredSize;
                        
                        // تأكد من أن الارتفاع يغطي كل البيانات
                        printArea.Arrange(new Rect(new Point(0, 0), contentSize));
                        printArea.UpdateLayout();

                        // 3. إنشاء حاوية الطباعة النهائية (Layout للورقة)
                        Grid printGrid = new Grid
                        {
                            Width = targetWidth,
                            Height = contentSize.Height,
                            FlowDirection = FlowDirection.RightToLeft, // ضمان العربي
                            Background = System.Windows.Media.Brushes.White
                        };

                        // 4. استخدام VisualBrush لالتقاط التقرير
                        VisualBrush contentBrush = new VisualBrush(printArea)
                        {
                            Stretch = Stretch.None,
                            AlignmentX = AlignmentX.Right, // المحاذاة لليمين
                            AlignmentY = AlignmentY.Top,
                            ViewboxUnits = BrushMappingMode.Absolute,
                            Viewbox = new Rect(0, 0, contentSize.Width, contentSize.Height)
                        };

                        // 5. وضع المحتوى في مستطيل داخل الـ Grid
                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                        {
                            Width = targetWidth,
                            Height = contentSize.Height,
                            Fill = contentBrush
                        };
                        printGrid.Children.Add(rect);

                        // 6. تجهيز الـ Visual للتوزيع على الورق
                        printGrid.Measure(new Size(targetWidth, contentSize.Height));
                        printGrid.Arrange(new Rect(new Point(0, 0), new Size(targetWidth, contentSize.Height)));
                        printGrid.UpdateLayout();

                        // 7. تطبيق التحجيم النهائي لتناسب الورقة
                        ContainerVisual visual = new ContainerVisual();
                        visual.Transform = new MatrixTransform(new Matrix(scale, 0, 0, scale, 20, 20));
                        visual.Children.Add(printGrid);

                        // 8. الطباعة
                        printDialog.PrintVisual(visual, $"تقرير المورد - {Report.SupplierName}");
                    }
                    finally
                    {
                        // 9. إعادة تهيئة الواجهة لضمان عدم تأثر الشاشة
                        printArea.InvalidateMeasure();
                        printArea.UpdateLayout();
                        IsLoading = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"عذراً، فشلت الطباعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                IsLoading = false;
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
