using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp
{
    public static class TokenStore
    {
        // يخزن الـ JWT Token بعد تسجيل الدخول
        public static string Token { get; set; } = string.Empty;
    }
}
