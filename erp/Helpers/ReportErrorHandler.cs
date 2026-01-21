using System;
using System.Net.Http;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;

namespace erp.Helpers
{
    public class ReportErrorState
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ActionText { get; set; }
        public PackIconKind IconKind { get; set; }
        public bool IsVisible { get; set; } = true;

        public static ReportErrorState Empty => new ReportErrorState { IsVisible = false };
    }

    public static class ReportErrorHandler
    {
        public static ReportErrorState HandleException(Exception ex)
        {
            // 1. Validation Errors
            if (ex is ArgumentException)
            {
                return new ReportErrorState
                {
                    Title = "بيانات البحث غير مكتملة",
                    Message = "يرجى التأكد من إدخال جميع البيانات المطلوبة بشكل صحيح.",
                    ActionText = "أكمل البيانات واضغط بحث",
                    IconKind = PackIconKind.ClipboardAlert
                };
            }

            // 2. HTTP Request Errors (Could be 404, 500, or actual Network)
            if (ex is HttpRequestException httpEx)
            {
                // Try to extract status code from the message if ApiClient throws it in "Request failed: 404..." format
                if (httpEx.Message.Contains("404"))
                {
                    return HandleApiError(404, "لم يتم العثور على البيانات المطلوبة.");
                }
                if (httpEx.Message.Contains("400"))
                {
                    return CreateValidation("البيانات المدخلة غير صحيحة، يرجى التحقق وإعادة المحاولة.");
                }
                if (httpEx.Message.Contains("401"))
                {
                    return HandleApiError(401, null);
                }
                if (httpEx.Message.Contains("403"))
                {
                    return HandleApiError(403, null);
                }
                if (httpEx.Message.Contains("500"))
                {
                    return HandleApiError(500, null);
                }

                // If no status code found, assume legitimate network error
                return new ReportErrorState
                {
                    Title = "لا يوجد اتصال بالإنترنت",
                    Message = "تعذر الوصول إلى الخادم. تأكد من أن السيرفر يعمل وأنك متصل بالشبكة.",
                    ActionText = "تحقق من الاتصال",
                    IconKind = PackIconKind.WifiOff
                };
            }

            // 3. Timeouts
            if (ex is TaskCanceledException || ex is TimeoutException)
            {
                return new ReportErrorState
                {
                    Title = "استغرق الطلب وقتاً أطول من المعتاد",
                    Message = "الخادم لم يستجب في الوقت المحدد.",
                    ActionText = "تحديث الصفحة",
                    IconKind = PackIconKind.TimerSand
                };
            }

            // 4. Unknown
            return new ReportErrorState
            {
                Title = "حدث خطأ غير متوقع",
                Message = $"تفاصيل الخطأ: {ex.Message}",
                ActionText = "تواصل مع الدعم الفني",
                IconKind = PackIconKind.AlertOctagon
            };
        }

        public static ReportErrorState HandleApiError(int statusCode, string serverMessage)
        {
            switch (statusCode)
            {
                case 401: // Unauthorized
                    return new ReportErrorState
                    {
                        Title = "الجلسة انتهت",
                        Message = "انتهت صلاحية تسجيل دخولك للحفاظ على الأمان.",
                        ActionText = "تسجيل الدخول",
                        IconKind = PackIconKind.AccountKey
                    };

                case 403: // Forbidden
                    return new ReportErrorState
                    {
                        Title = "غير مصرح لك بالوصول",
                        Message = "ليس لديك الصلاحيات اللازمة لعرض هذا التقرير.",
                        ActionText = "تواصل مع الإدارة",
                        IconKind = PackIconKind.ShieldLock
                    };

                case 404: // Not Found (Data)
                    return new ReportErrorState
                    {
                        Title = "لا توجد بيانات للعرض",
                        Message = "لم يتم العثور على أي سجلات تطابق معايير البحث.",
                        ActionText = "جرب تغيير البحث",
                        IconKind = PackIconKind.FileSearchOutline
                    };

                case 500: // Server Error
                case 503:
                    return new ReportErrorState
                    {
                        Title = "حدث خطأ في الخادم",
                        Message = "واجهنا مشكلة فنية في الخادم تمنع إتمام طلبك حالياً.",
                        ActionText = "حاول مرة أخرى لاحقاً",
                        IconKind = PackIconKind.ServerNetworkOff
                    };

                default:
                    // Usually business logic error returned by API
                    return new ReportErrorState
                    {
                        Title = "تنبيه من النظام",
                        Message = string.IsNullOrWhiteSpace(serverMessage) ? "حدث خطأ غير معروف" : serverMessage,
                        ActionText = "حاول مرة أخرى",
                        IconKind = PackIconKind.AlertCircleOutline
                    };
            }
        }

        public static ReportErrorState CreateValidation(string message)
        {
            return new ReportErrorState
            {
                Title = "بيانات غير صالحة",
                Message = message,
                ActionText = "صحح الخطأ",
                IconKind = PackIconKind.AlertCircle
            };
        }
    }
}
