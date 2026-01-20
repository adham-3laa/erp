using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace erp.Services
{
    /// <summary>
    /// Centralized error handling service that provides:
    /// - User-friendly Arabic error messages
    /// - Internal error logging for developers
    /// - Consistent error handling across the application
    /// </summary>
    public static class ErrorHandlingService
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ERP", "Logs");

        private static readonly object _logLock = new();

        #region Arabic Error Messages

        /// <summary>
        /// Common Arabic error messages for various error types
        /// </summary>
        public static class Messages
        {
            // Network & Connection Errors
            public const string NetworkError = "خطأ في الاتصال بالخادم. تحقق من اتصالك بالإنترنت وحاول مرة أخرى.";
            public const string ServerUnavailable = "الخادم غير متاح حالياً. يرجى المحاولة لاحقاً.";
            public const string ConnectionTimeout = "انتهت مهلة الاتصال. تحقق من اتصالك بالإنترنت وحاول مرة أخرى.";
            public const string RequestCancelled = "تم إلغاء العملية.";

            // Authentication Errors
            public const string Unauthorized = "انتهت صلاحية الجلسة. يرجى تسجيل الدخول مرة أخرى.";
            public const string Forbidden = "ليس لديك صلاحية للوصول إلى هذا المورد.";
            public const string InvalidCredentials = "بيانات الدخول غير صحيحة. تحقق من البريد الإلكتروني وكلمة المرور.";

            // Data & Validation Errors
            public const string InvalidData = "البيانات المدخلة غير صحيحة. يرجى التحقق والمحاولة مرة أخرى.";
            public const string DataNotFound = "لم يتم العثور على البيانات المطلوبة.";
            public const string DuplicateEntry = "هذا العنصر موجود بالفعل.";
            public const string RequiredFieldMissing = "يرجى ملء جميع الحقول المطلوبة.";

            // Operation Errors  
            public const string OperationFailed = "فشلت العملية. يرجى المحاولة مرة أخرى.";
            public const string SaveFailed = "فشل في حفظ البيانات. يرجى المحاولة مرة أخرى.";
            public const string DeleteFailed = "فشل في حذف البيانات. يرجى المحاولة مرة أخرى.";
            public const string LoadFailed = "فشل في تحميل البيانات. يرجى المحاولة مرة أخرى.";
            public const string UpdateFailed = "فشل في تحديث البيانات. يرجى المحاولة مرة أخرى.";

            // Cheque-Specific Errors
            public const string ChequeLoadError = "خطأ في تحميل الشيكات. يرجى المحاولة مرة أخرى.";
            public const string ChequeAddError = "فشل في إضافة الشيك. يرجى التحقق من البيانات والمحاولة مرة أخرى.";
            public const string ChequeUpdateError = "فشل في تحديث الشيك. يرجى المحاولة مرة أخرى.";
            public const string ChequeStatusUpdateError = "فشل في تحديث حالة الشيك. يرجى المحاولة مرة أخرى.";

            // General
            public const string UnexpectedError = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى أو الاتصال بالدعم الفني.";
            public const string ContactSupport = "إذا استمرت المشكلة، يرجى الاتصال بالدعم الفني.";
        }

        #endregion

        #region Exception to Arabic Message Translation

        /// <summary>
        /// Translates an exception to a user-friendly Arabic message
        /// </summary>
        public static string GetUserFriendlyMessage(Exception ex, string? contextMessage = null)
        {
            // Log the full exception for developers
            LogError(ex, contextMessage);

            // Return appropriate Arabic message based on exception type
            return ex switch
            {
                // Network & HTTP Errors
                HttpRequestException httpEx => GetHttpErrorMessage(httpEx),
                TaskCanceledException => Messages.ConnectionTimeout,
                OperationCanceledException => Messages.RequestCancelled,
                SocketException => Messages.NetworkError,

                // JSON/Data Errors
                JsonException => Messages.InvalidData,
                FormatException => Messages.InvalidData,
                ArgumentException => Messages.InvalidData,

                // Null/Not Found
                NullReferenceException => Messages.DataNotFound,
                InvalidOperationException => Messages.OperationFailed,

                // File/IO Errors
                IOException => Messages.OperationFailed,

                // Default - use context message if provided
                _ => contextMessage ?? Messages.UnexpectedError
            };
        }

        /// <summary>
        /// Gets appropriate Arabic message for HTTP errors based on status code
        /// </summary>
        private static string GetHttpErrorMessage(HttpRequestException ex)
        {
            // Try to extract status code from message (since HttpRequestException may contain it)
            var message = ex.Message?.ToLower() ?? "";

            if (message.Contains("401") || message.Contains("unauthorized"))
                return Messages.Unauthorized;

            if (message.Contains("403") || message.Contains("forbidden"))
                return Messages.Forbidden;

            if (message.Contains("404") || message.Contains("not found"))
                return Messages.DataNotFound;

            if (message.Contains("400") || message.Contains("bad request"))
                return Messages.InvalidData;

            if (message.Contains("409") || message.Contains("conflict"))
                return Messages.DuplicateEntry;

            if (message.Contains("500") || message.Contains("internal server"))
                return Messages.ServerUnavailable;

            if (message.Contains("502") || message.Contains("503") || message.Contains("504"))
                return Messages.ServerUnavailable;

            if (message.Contains("timeout"))
                return Messages.ConnectionTimeout;

            // Check for network-level errors
            if (ex.InnerException is SocketException || 
                message.Contains("connection") || 
                message.Contains("network"))
                return Messages.NetworkError;

            return Messages.NetworkError;
        }

        /// <summary>
        /// Gets Arabic message for a specific HTTP status code
        /// </summary>
        public static string GetMessageForStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.Unauthorized => Messages.Unauthorized,
                HttpStatusCode.Forbidden => Messages.Forbidden,
                HttpStatusCode.NotFound => Messages.DataNotFound,
                HttpStatusCode.BadRequest => Messages.InvalidData,
                HttpStatusCode.Conflict => Messages.DuplicateEntry,
                HttpStatusCode.InternalServerError => Messages.ServerUnavailable,
                HttpStatusCode.BadGateway => Messages.ServerUnavailable,
                HttpStatusCode.ServiceUnavailable => Messages.ServerUnavailable,
                HttpStatusCode.GatewayTimeout => Messages.ConnectionTimeout,
                HttpStatusCode.RequestTimeout => Messages.ConnectionTimeout,
                _ => Messages.OperationFailed
            };
        }

        #endregion

        #region Logging

        /// <summary>
        /// Logs an error with full technical details for developers (never shown to users)
        /// </summary>
        public static void LogError(Exception ex, string? context = null)
        {
            try
            {
                EnsureLogDirectoryExists();

                var logFile = Path.Combine(LogDirectory, $"error_{DateTime.Now:yyyy-MM-dd}.log");
                var logEntry = FormatLogEntry(ex, context);

                lock (_logLock)
                {
                    File.AppendAllText(logFile, logEntry);
                }

                // Also log to Debug output for development
                System.Diagnostics.Debug.WriteLine($"[ERROR] {context}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            catch
            {
                // Silently fail logging - never let logging break the app
            }
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public static void LogInfo(string message)
        {
            try
            {
                EnsureLogDirectoryExists();

                var logFile = Path.Combine(LogDirectory, $"info_{DateTime.Now:yyyy-MM-dd}.log");
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}{Environment.NewLine}";

                lock (_logLock)
                {
                    File.AppendAllText(logFile, logEntry);
                }

                System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
            }
            catch
            {
                // Silently fail
            }
        }

        private static void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        private static string FormatLogEntry(Exception ex, string? context)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Context: {context ?? "N/A"}");
            sb.AppendLine($"Exception Type: {ex.GetType().FullName}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Stack Trace:");
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                sb.AppendLine($"--- Inner Exception ---");
                sb.AppendLine($"Type: {ex.InnerException.GetType().FullName}");
                sb.AppendLine($"Message: {ex.InnerException.Message}");
                sb.AppendLine($"Stack Trace: {ex.InnerException.StackTrace}");
            }

            sb.AppendLine();
            return sb.ToString();
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Shows an error message to the user in Arabic
        /// </summary>
        public static void ShowError(string arabicMessage, string title = "خطأ")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    arabicMessage,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            });
        }

        /// <summary>
        /// Shows a warning message to the user in Arabic
        /// </summary>
        public static void ShowWarning(string arabicMessage, string title = "تنبيه")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    arabicMessage,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            });
        }

        /// <summary>
        /// Shows a success message to the user in Arabic
        /// </summary>
        public static void ShowSuccess(string arabicMessage, string title = "نجاح")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    arabicMessage,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            });
        }

        /// <summary>
        /// Shows a confirmation dialog in Arabic and returns true if user confirms
        /// </summary>
        public static bool Confirm(string arabicMessage, string title = "تأكيد")
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(
                    arabicMessage,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return result == MessageBoxResult.Yes;
            });
        }

        #endregion

        #region Safe Execution Wrappers

        /// <summary>
        /// Safely executes an async operation with error handling
        /// </summary>
        public static async Task<(bool Success, T? Result, string? ErrorMessage)> SafeExecuteAsync<T>(
            Func<Task<T>> operation,
            string errorContext)
        {
            try
            {
                var result = await operation();
                return (true, result, null);
            }
            catch (Exception ex)
            {
                var message = GetUserFriendlyMessage(ex, errorContext);
                return (false, default, message);
            }
        }

        /// <summary>
        /// Safely executes an async operation with error handling (no return value)
        /// </summary>
        public static async Task<(bool Success, string? ErrorMessage)> SafeExecuteAsync(
            Func<Task> operation,
            string errorContext)
        {
            try
            {
                await operation();
                return (true, null);
            }
            catch (Exception ex)
            {
                var message = GetUserFriendlyMessage(ex, errorContext);
                return (false, message);
            }
        }

        /// <summary>
        /// Safely executes a synchronous operation with error handling
        /// </summary>
        public static (bool Success, T? Result, string? ErrorMessage) SafeExecute<T>(
            Func<T> operation,
            string errorContext)
        {
            try
            {
                var result = operation();
                return (true, result, null);
            }
            catch (Exception ex)
            {
                var message = GetUserFriendlyMessage(ex, errorContext);
                return (false, default, message);
            }
        }

        #endregion
    }
}
