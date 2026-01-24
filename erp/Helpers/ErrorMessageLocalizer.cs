using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace erp.Helpers
{
    /// <summary>
    /// مساعد لتعريب رسائل الأخطاء القادمة من API
    /// </summary>
    public static class ErrorMessageLocalizer
    {
        // قاموس الترجمات للأخطاء الشائعة
        private static readonly Dictionary<string, string> ErrorTranslations = new()
        {
            // Stock Errors
            { "Insufficient stock", "الكمية غير كافية" },
            { "insufficient stock", "الكمية غير كافية" },
            { "Out of stock", "المنتج غير متوفر في المخزون" },
            { "out of stock", "المنتج غير متوفر في المخزون" },
            { "Stock not available", "المخزون غير متاح" },
            { "stock not available", "المخزون غير متاح" },
            { "Product not found", "المنتج غير موجود" },
            { "product not found", "المنتج غير موجود" },
            
            // Validation Errors
            { "Invalid input", "البيانات المدخلة غير صحيحة" },
            { "invalid input", "البيانات المدخلة غير صحيحة" },
            { "Required field", "هذا الحقل مطلوب" },
            { "required field", "هذا الحقل مطلوب" },
            { "Invalid format", "الصيغة غير صحيحة" },
            { "invalid format", "الصيغة غير صحيحة" },
            
            // User/Auth Errors
            { "User not found", "المستخدم غير موجود" },
            { "user not found", "المستخدم غير موجود" },
            { "Unauthorized", "غير مصرح لك بهذا الإجراء" },
            { "unauthorized", "غير مصرح لك بهذا الإجراء" },
            { "Access denied", "تم رفض الوصول" },
            { "access denied", "تم رفض الوصول" },
            { "Invalid credentials", "بيانات الدخول غير صحيحة" },
            { "invalid credentials", "بيانات الدخول غير صحيحة" },
            
            // Network Errors
            { "Network error", "خطأ في الاتصال بالشبكة" },
            { "network error", "خطأ في الاتصال بالشبكة" },
            { "Connection failed", "فشل الاتصال" },
            { "connection failed", "فشل الاتصال" },
            { "Timeout", "انتهت مهلة الاتصال" },
            { "timeout", "انتهت مهلة الاتصال" },
            { "Server error", "خطأ في الخادم" },
            { "server error", "خطأ في الخادم" },
            
            // General
            { "Something went wrong", "حدث خطأ ما" },
            { "something went wrong", "حدث خطأ ما" },
            { "Error occurred", "حدث خطأ" },
            { "error occurred", "حدث خطأ" },
            { "Failed", "فشلت العملية" },
            { "failed", "فشلت العملية" },
            { "Success", "نجحت العملية" },
            { "success", "نجحت العملية" },
            
            // Order specific
            { "Order not found", "الطلب غير موجود" },
            { "order not found", "الطلب غير موجود" },
            { "Customer not found", "العميل غير موجود" },
            { "customer not found", "العميل غير موجود" },
            { "Sales representative not found", "المندوب غير موجود" },
            { "sales representative not found", "المندوب غير موجود" },
            
            // Keywords for product
            { "for product", "للمنتج" },
            { "Product", "المنتج" },
            { "product", "المنتج" },
        };

        /// <summary>
        /// تعريب رسالة الخطأ
        /// </summary>
        /// <param name="errorMessage">رسالة الخطأ الأصلية</param>
        /// <returns>رسالة الخطأ المعربة</returns>
        public static string LocalizeError(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                return "حدث خطأ غير معروف";

            try
            {
                // استخراج الجزء المهم من رسالة الخطأ
                var cleanMessage = CleanErrorMessage(errorMessage);
                
                // محاولة معالجة رسائل خاصة بنقص المخزون
                var localizedMessage = LocalizeInsufficientStockError(cleanMessage);
                if (localizedMessage != cleanMessage)
                    return localizedMessage;

                // معالجة رسائل خطأ أخرى خاصة بالمنتجات
                localizedMessage = LocalizeProductErrors(cleanMessage);
                if (localizedMessage != cleanMessage)
                    return localizedMessage;

                // معالجة أخطاء المستخدمين (Sales Rep, Customer, etc)
                localizedMessage = LocalizeUserErrors(cleanMessage);
                if (localizedMessage != cleanMessage)
                    return localizedMessage;

                // الترجمة العامة للرسالة
                localizedMessage = TranslateMessage(cleanMessage);
                
                return localizedMessage;
            }
            catch
            {
                // في حالة فشل التعريب، نعيد رسالة عامة
                return "حدث خطأ أثناء معالجة الطلب";
            }
        }

        /// <summary>
        /// تنظيف رسالة الخطأ من البيانات الإضافية
        /// </summary>
        private static string CleanErrorMessage(string errorMessage)
        {
            // إزالة Status Code و ReasonPhrase
            // مثال: "Request failed: 400 Bad Request\nInsufficient stock for product 'شاشه'"
            
            // البحث عن النص بعد \n إذا كان موجوداً
            if (errorMessage.Contains("\n"))
            {
                var parts = errorMessage.Split('\n');
                // نأخذ الجزء الثاني إذا لم يكن فارغاً
                for (int i = 1; i < parts.Length; i++)
                {
                    var part = parts[i].Trim();
                    if (!string.IsNullOrWhiteSpace(part) && !part.StartsWith("Request failed"))
                    {
                        errorMessage = part;
                        break;
                    }
                }
            }

            // إزالة "Request failed:" إذا كان موجوداً
            errorMessage = Regex.Replace(errorMessage, @"Request failed:\s*\d+\s+\w+", "").Trim();

            // إزالة JSON brackets والنقطة في النهاية إذا كانت موجودة
            errorMessage = errorMessage.Trim('{', '}', '[', ']', '"').Trim();
            errorMessage = errorMessage.TrimEnd('.');

            return errorMessage;
        }

        /// <summary>
        /// معالجة خاصة لأخطاء نقص المخزون
        /// </summary>
        private static string LocalizeInsufficientStockError(string message)
        {
            // Pattern: "Insufficient stock for product 'ProductName'"
            var pattern = @"Insufficient stock for product ['""'](.+?)['""']";
            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var productName = match.Groups[1].Value;
                return $"الكمية المتوفرة غير كافية للمنتج '{productName}'";
            }

            // التحقق من وجود عبارة "insufficient stock" بدون اسم المنتج
            if (Regex.IsMatch(message, @"insufficient\s+stock", RegexOptions.IgnoreCase))
            {
                return "الكمية المتوفرة غير كافية";
            }

            return message;
        }

        /// <summary>
        /// معالجة أخطاء المنتجات الأخرى
        /// </summary>
        private static string LocalizeProductErrors(string message)
        {
            // Pattern 1: "Product with Name 'شاشششه' not found" or "Product with Name شاشششه not found"
            var productWithNamePattern = @"Product\s+with\s+Name\s+['""]*(.+?)['""]*\s+not found";
            var match = Regex.Match(message, productWithNamePattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var productName = match.Groups[1].Value.Trim().Trim('\'', '"', '.');
                return $"المنتج '{productName}' غير موجود";
            }

            // Pattern 2: "Product 'name' not found"
            var productNotFoundPattern = @"Product\s+['""]+(.+?)['""]+\s+not found";
            match = Regex.Match(message, productNotFoundPattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var productName = match.Groups[1].Value;
                return $"المنتج '{productName}' غير موجود";
            }

            // Pattern 3: "Product name not found" (without quotes)
            var simpleProductPattern = @"Product\s+(.+?)\s+not found";
            match = Regex.Match(message, simpleProductPattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var productName = match.Groups[1].Value.Trim();
                // تجنب الكلمات الإنجليزية العامة
                if (!productName.Equals("with", StringComparison.OrdinalIgnoreCase) && 
                    !productName.Equals("name", StringComparison.OrdinalIgnoreCase) &&
                    productName.Length > 1)
                {
                    return $"المنتج '{productName}' غير موجود";
                }
            }

            // Pattern 4: Out of stock error
            var outOfStockPattern = @"Product\s+['""]*(.+?)['""]*\s+(is\s+)?out of stock";
            match = Regex.Match(message, outOfStockPattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var productName = match.Groups[1].Value.Trim().Trim('\'', '"');
                return $"المنتج '{productName}' غير متوفر في المخزون";
            }

            return message;
        }

        /// <summary>
        /// معالجة أخطاء المستخدمين (Sales Representative, Customer, User, etc)
        /// </summary>
        private static string LocalizeUserErrors(string message)
        {
            // Pattern 1: "Sales Representative not found" or "Sales Representative with name X not found"
            if (Regex.IsMatch(message, @"Sales\s+Representative.*not\s+found", RegexOptions.IgnoreCase))
            {
                // Try to extract name if available
                var namePattern = @"Sales\s+Representative\s+with\s+name\s+['""]*(.+?)['""]*\s+not\s+found";
                var match = Regex.Match(message, namePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var userName = match.Groups[1].Value.Trim().Trim('\'', '"', '.');
                    return $"المندوب '{userName}' غير موجود";
                }
                return "المندوب غير موجود";
            }

            // Pattern 2: "Customer not found" or "Customer with name X not found"
            if (Regex.IsMatch(message, @"Customer.*not\s+found", RegexOptions.IgnoreCase))
            {
                var namePattern = @"Customer\s+with\s+name\s+['""]*(.+?)['""]*\s+not\s+found";
                var match = Regex.Match(message, namePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var userName = match.Groups[1].Value.Trim().Trim('\'', '"', '.');
                    return $"العميل '{userName}' غير موجود";
                }
                return "العميل غير موجود";
            }

            // Pattern 3: "User not found" or "User with name/id X not found"
            if (Regex.IsMatch(message, @"User.*not\s+found", RegexOptions.IgnoreCase))
            {
                var namePattern = @"User\s+with\s+(name|id)\s+['""]*(.+?)['""]*\s+not\s+found";
                var match = Regex.Match(message, namePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var userName = match.Groups[2].Value.Trim().Trim('\'', '"', '.');
                    return $"المستخدم '{userName}' غير موجود";
                }
                return "المستخدم غير موجود";
            }

            // Pattern 4: "Supplier not found"
            if (Regex.IsMatch(message, @"Supplier.*not\s+found", RegexOptions.IgnoreCase))
            {
                var namePattern = @"Supplier\s+with\s+name\s+['""]*(.+?)['""]*\s+not\s+found";
                var match = Regex.Match(message, namePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var supplierName = match.Groups[1].Value.Trim().Trim('\'', '"', '.');
                    return $"المورد '{supplierName}' غير موجود";
                }
                return "المورد غير موجود";
            }

            return message;
        }

        /// <summary>
        /// ترجمة الرسالة باستخدام القاموس
        /// </summary>
        private static string TranslateMessage(string message)
        {
            var translatedMessage = message;

            // الترجمة الكاملة أولاً
            if (ErrorTranslations.ContainsKey(message))
            {
                return ErrorTranslations[message];
            }

            // ترجمة الكلمات الموجودة في الرسالة
            foreach (var translation in ErrorTranslations)
            {
                if (translatedMessage.Contains(translation.Key))
                {
                    translatedMessage = translatedMessage.Replace(translation.Key, translation.Value);
                }
            }

            // إذا لم تتغير الرسالة، نعيد رسالة عامة أكثر وضوحاً
            if (translatedMessage == message && IsEnglish(message))
            {
                return $"حدث خطأ: {message}";
            }

            return translatedMessage;
        }

        /// <summary>
        /// التحقق من أن النص باللغة الإنجليزية
        /// </summary>
        private static bool IsEnglish(string text)
        {
            return Regex.IsMatch(text, @"[a-zA-Z]");
        }

        /// <summary>
        /// استخراج رسالة الخطأ من Exception
        /// </summary>
        public static string GetLocalizedErrorFromException(Exception ex)
        {
            if (ex == null)
                return "حدث خطأ غير معروف";

            var errorMessage = ex.Message;

            // معالجة HttpRequestException خاصة
            if (ex is System.Net.Http.HttpRequestException)
            {
                errorMessage = LocalizeError(errorMessage);
            }
            // معالجة أنواع أخرى من الأخطاء
            else if (ex is InvalidOperationException)
            {
                errorMessage = "عملية غير صالحة: " + LocalizeError(errorMessage);
            }
            else if (ex is ArgumentException)
            {
                errorMessage = "خطأ في البيانات المدخلة: " + LocalizeError(errorMessage);
            }
            else
            {
                errorMessage = LocalizeError(errorMessage);
            }

            return errorMessage;
        }
    }
}
