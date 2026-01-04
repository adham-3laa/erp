using erp.ViewModels.Returns;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Returns
{
    public partial class ReturnsOrderItemsView : Page
    {
        private readonly ReturnsOrderItemsViewModel _vm;
        private readonly CreateReturnViewModel _createReturnVm;

        public ReturnsOrderItemsView(
            ReturnsOrderItemsViewModel vm,
            CreateReturnViewModel createReturnVm)
        {
            InitializeComponent();
            _vm = vm;
            _createReturnVm = createReturnVm;
            DataContext = _vm; // ربط الـ ViewModel
        }

        // دالة لتحميل بيانات الـ OrderItems بناءً على الـ orderId المدخل
        private async void Load_Click(object sender, RoutedEventArgs e)
{
    var orderId = OrderIdTextBoxInput.Text?.Trim(); // استخدام الاسم الجديد

    // تحقق إذا كان الـ orderId فارغ أو غير صحيح
    if (string.IsNullOrWhiteSpace(orderId))
    {
        MessageBox.Show("من فضلك أدخل رقم الطلب");
        return;
    }

    // تحميل بيانات الـ OrderItems بناءً على الـ orderId
    await _vm.LoadOrderItemsAsync(orderId); // استدعاء دالة الـ ViewModel بعد تعديل مستوى الوصول
}


        // العودة إلى الصفحة السابقة
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        // الانتقال إلى صفحة إنشاء الإرجاع
        private void CreateReturn_Click(object sender, RoutedEventArgs e)
        {
            var view = new CreateReturnView(_createReturnVm); // استخدام ViewModel الخاص بإنشاء الإرجاع
            NavigationService?.Navigate(view); // التنقل إلى صفحة الإرجاع
        }
    }
}
