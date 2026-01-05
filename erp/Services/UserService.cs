using erp.DTOS;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace erp.Services
{
    public class UserService
    {
        private readonly ApiClient _api;

        public UserService(ApiClient api)
        {
            _api = api;
        }

        // ===================== CREATE USER =====================
        public async Task<CreateUserResponseDto> CreateUserAsync(UserPostDto userDto)
        {
            return await _api.PostAsync<CreateUserResponseDto>(
    "api/users/create",
    userDto
);

        }

        // ===================== UPDATE USER =====================
        public async Task<UserDto> UpdateUserAsync(string id, UserUpdateDto updateDto)
        {
            return await _api.PutAsync<UserDto>(
                $"api/users/update?id={id}",
                updateDto
            );
        }

        // ===================== GET USERS =====================
        public async Task<UserResponseDto> GetUsersAsync(
            string search = null,
            string userType = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrWhiteSpace(search))
                query["search"] = search;

            if (!string.IsNullOrWhiteSpace(userType))
                query["userType"] = userType;

            if (isActive.HasValue)
                query["isActive"] = isActive.Value.ToString();

            query["page"] = page.ToString();
            query["pageSize"] = pageSize.ToString();

            return await _api.GetAsync<UserResponseDto>(
                $"api/users/list?{query}"
            );
        }

        // ===================== CHANGE PASSWORD =====================
        public async Task<ChangePasswordResponseDto> ChangePasswordAsync(string userId, string newPassword)
        {
            return await _api.PatchAsync<ChangePasswordResponseDto>(
                $"api/users/change-password?id={userId}",
                newPassword
            );
        }

        // ===================== TOGGLE STATUS =====================
        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            await _api.PatchAsync<object>(
                $"api/users/toggle-active?id={userId}",
                new { }
            );

            return true;
        }

        // ===================== DELETE =====================
        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                await _api.DeleteAsync($"api/users/delete?id={userId}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ===================== GET BY ID (قديم) =====================
        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            return await _api.GetAsync<UserDto>(
                $"api/users/get-user?id={id}"
            );
        }

        // ✅ ===================== GET USER DETAILS (مطابق للريسبونس) =====================
        public async Task<CurrentUserDto?> GetUserDetailsByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return await _api.GetAsync<CurrentUserDto>(
                $"api/users/get-user?id={Uri.EscapeDataString(id.Trim())}"
            );
        }


        // ✅ ===================== GET CURRENT USER (يعتمد على get-user) =====================
        public async Task<CurrentUserDto?> GetCurrentUserAsync()
        {
            try
            {
                Debug.WriteLine($"[UserService] [{DateTime.Now:HH:mm:ss}] تحميل المستخدم الحالي عبر get-user ...");

                var currentUserId = TryGetUserIdFromSession(App.Session);

                if (!string.IsNullOrWhiteSpace(currentUserId))
                {
                    var byId = await GetUserDetailsByIdAsync(currentUserId);
                    if (byId != null && byId.StatusCode == 200)
                    {
                        Debug.WriteLine($"[UserService] get-user OK | {byId.Fullname} | {byId.UserType}");
                        return byId;
                    }
                }

                Debug.WriteLine("[UserService] لم يتم العثور على UserId في Session/JWT");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UserService] خطأ: {ex.Message}");
                return null;
            }
        }


        // ===================== Helpers: Get UserId from Session / JWT =====================
        private static string? TryGetUserIdFromSession(object? session)
        {
            if (session == null) return null;

            try
            {
                // 1) جرّب property names مباشرة
                var id = TryReadStringProperty(session, "UserId")
                      ?? TryReadStringProperty(session, "UserID")
                      ?? TryReadStringProperty(session, "Id")
                      ?? TryReadStringProperty(session, "ID");

                if (!string.IsNullOrWhiteSpace(id))
                    return id;

                // 2) جرّب session.User.Id / session.User.UserId
                var userObj = TryReadObjectProperty(session, "User")
                           ?? TryReadObjectProperty(session, "CurrentUser")
                           ?? TryReadObjectProperty(session, "Account");

                if (userObj != null)
                {
                    id = TryReadStringProperty(userObj, "Id")
                      ?? TryReadStringProperty(userObj, "ID")
                      ?? TryReadStringProperty(userObj, "UserId")
                      ?? TryReadStringProperty(userObj, "UserID");

                    if (!string.IsNullOrWhiteSpace(id))
                        return id;
                }

                // 3) جرّب قراءة JWT من session.AccessToken / Token / Jwt / BearerToken...
                var token = TryReadStringProperty(session, "AccessToken")
                         ?? TryReadStringProperty(session, "Token")
                         ?? TryReadStringProperty(session, "Jwt")
                         ?? TryReadStringProperty(session, "JwtToken")
                         ?? TryReadStringProperty(session, "BearerToken");

                if (string.IsNullOrWhiteSpace(token))
                    return null;

                token = token.Trim();
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = token.Substring("Bearer ".Length).Trim();

                return TryGetUserIdFromJwt(token);
            }
            catch
            {
                return null;
            }
        }

        private static object? TryReadObjectProperty(object obj, string propName)
        {
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(obj);
        }

        private static string? TryReadStringProperty(object obj, string propName)
        {
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return null;

            var val = prop.GetValue(obj);
            return val?.ToString();
        }

        private static string? TryGetUserIdFromJwt(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2) return null;

                var payloadJson = Base64UrlDecodeToString(parts[1]);
                using var doc = JsonDocument.Parse(payloadJson);
                var root = doc.RootElement;

                // ✅ نفس اللي ظاهر عندك في التوكن
                if (root.TryGetProperty("sub", out var sub))
                    return sub.GetString();

                const string nameId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
                if (root.TryGetProperty(nameId, out var nameIdentifier))
                    return nameIdentifier.GetString();

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string Base64UrlDecodeToString(string input)
        {
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }

            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }
    }

}
