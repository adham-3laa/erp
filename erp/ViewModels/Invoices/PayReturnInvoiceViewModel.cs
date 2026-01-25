using CommunityToolkit.Mvvm.Input;
using erp.DTOS.InvoicesDTOS;
using erp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels.Invoices
{
    /// <summary>
    /// ViewModel مخصص فقط لفواتير المرتجعات (Return Invoices)
    /// يفصل منطق الدفع عن باقي الفواتير لتجنب المشاكل
    /// </summary>
    public class PayReturnInvoiceViewModel : INotifyPropertyChanged
    {
        private readonly InvoicePaymentService _service;
        private readonly InvoiceResponseDto _invoice;

        public PayReturnInvoiceViewModel(InvoiceResponseDto invoice)
        {
            _service = new InvoicePaymentService();
            _invoice = invoice;

            // ✅ تصحيح جذري: الرقم 46 هو Invoice Code وليس Order Code
            // الـ API يطلب باراميتر اسمه orderId لكنه يتوقع كود الفاتورة في حالة المرتجعات!
            OrderCode = invoice.code.ToString();
            
            // ✅ استخدام "Customer" كنوع (بناءً على طلبك وتجربة الـ Curl الناجحة)
            // إذا كان المرتجع يعامل معاملة العميل في الباك إند
            TargetType = "Customer"; 

            RemainingAmount = invoice.RemainingAmount;

            PayCommand = new RelayCommand(async () => await Pay(), CanPay);
        }

        public string OrderCode { get; }
        public string TargetType { get; }

        private decimal _remainingAmount;
        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set { _remainingAmount = value; OnPropertyChanged(); }
        }

        private decimal _paidAmount;
        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                if (_paidAmount != value)
                {
                    _paidAmount = value;
                    OnPropertyChanged();

                    // Sync text only if actual value mismatch (supports programmatic reset)
                    if (!decimal.TryParse(_paidAmountText, out var currentVal) || currentVal != value)
                    {
                        _paidAmountText = value == 0 ? "" : value.ToString();
                        OnPropertyChanged(nameof(PaidAmountText));
                    }

                    PayCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _paidAmountText = "";
        public string PaidAmountText
        {
            get => _paidAmountText;
            set
            {
                if (_paidAmountText != value)
                {
                    _paidAmountText = value;
                    OnPropertyChanged();

                    if (decimal.TryParse(value, out var val))
                    {
                        _paidAmount = val;
                    }
                    else
                    {
                        _paidAmount = 0;
                    }
                    OnPropertyChanged(nameof(PaidAmount));
                    PayCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }

        public RelayCommand PayCommand { get; }

        private bool CanPay()
            => PaidAmount > 0 && PaidAmount <= RemainingAmount && !IsLoading && !string.IsNullOrEmpty(OrderCode);

        private async Task Pay()
        {
            if (!erp.Views.Shared.ThemedDialog.ShowConfirmation(null, "تأكيد الدفع", $"هل أنت متأكد من دفع مبلغ {PaidAmount:N2}؟", "نعم", "لا"))
            {
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                SuccessMessage = null;

                // استدعاء الـ API بالقيم الصحيحة
                // TargetType = "Customer"
                // OrderId = "46" (OrderCode)
                var result = await _service.PayedAmountFromCustomerByOrderID(
                    TargetType,
                    OrderCode, 
                    PaidAmount
                );

                // تحديث القيم بعد النجاح
                RemainingAmount = result.RemainingAmount;
                
                // تحديث الكائن الأصلي للفاتورة لتنعكس التغييرات في باقي الصفحات
                if (_invoice != null)
                {
                    _invoice.PaidAmount = result.PaidAmount;
                    _invoice.RemainingAmount = result.RemainingAmount;
                }

                PaidAmount = 0;
                SuccessMessage = "تمت عملية الدفع بنجاح";
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"حدث خطأ أثناء الدفع: {ex.Message}";
                
                // في حالة الخطأ، جرب إرسال "Return" بدلاً من "Customer" كمحاولة تلقائية
                // (احتياطي فقط إذا كان الباك إند يتطلب ذلك فجأة)
                if (ex.Message.Contains("Not Found") && TargetType == "Customer")
                {
                     // يمكن إضافة منطق إعادة المحاولة هنا إذا لزم الأمر
                }
            }
            finally
            {
                IsLoading = false;
                PayCommand.NotifyCanExecuteChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
