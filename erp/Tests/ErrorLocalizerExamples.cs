using erp.Helpers;
using System;

namespace erp.Tests
{
    /// <summary>
    /// أمثلة اختبار لتعريب رسائل الأخطاء
    /// </summary>
    public class ErrorLocalizerExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== اختبار تعريب رسائل الأخطاء ===\n");

            // مثال 1: خطأ نقص المخزون
            TestError(
                "Request failed: 400 Bad Request\nInsufficient stock for product 'شاشه'",
                "خطأ نقص المخزون"
            );

            // مثال 2: خطأ نقص المخزون بدون اسم المنتج
            TestError(
                "Insufficient stock",
                "خطأ نقص المخزون (عام)"
            );

            // مثال 3: منتج غير موجود
            TestError(
                "Product 'لابتوب' not found",
                "منتج غير موجود"
            );

            // مثال 4: منتج غير متوفر
            TestError(
                "Product 'ماوس' is out of stock",
                "منتج غير متوفر"
            );

            // مثال 5: خطأ شبكة
            TestError(
                "Request failed: 500 Internal Server Error\nNetwork error",
                "خطأ شبكة"
            );

            // مثال 6: غير مصرح
            TestError(
                "Request failed: 401 Unauthorized\nUnauthorized access",
                "غير مصرح"
            );

            // مثال 7: بيانات غير صحيحة
            TestError(
                "Invalid input data",
                "بيانات غير صحيحة"
            );

            // مثال 8: حقل مطلوب
            TestError(
                "Required field missing",
                "حقل مطلوب"
            );

            Console.WriteLine("\n=== انتهى الاختبار ===");
        }

        private static void TestError(string originalError, string testName)
        {
            Console.WriteLine($"--- {testName} ---");
            Console.WriteLine($"الأصلي: {originalError}");
            
            var localized = ErrorMessageLocalizer.LocalizeError(originalError);
            Console.WriteLine($"المعرب: {localized}");
            Console.WriteLine();
        }

        // اختبار مع Exception
        public static void TestWithException()
        {
            Console.WriteLine("=== اختبار مع Exception ===\n");

            try
            {
                // محاكاة HttpRequestException
                throw new System.Net.Http.HttpRequestException(
                    "Request failed: 400 Bad Request\nInsufficient stock for product 'شاشه'"
                );
            }
            catch (Exception ex)
            {
                var localized = ErrorMessageLocalizer.GetLocalizedErrorFromException(ex);
                Console.WriteLine($"الخطأ الأصلي: {ex.Message}");
                Console.WriteLine($"الخطأ المعرب: {localized}");
            }
        }
    }
}
