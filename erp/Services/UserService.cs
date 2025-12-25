using erp.DTOS;
using System.Diagnostics;
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
        public async Task<UserDto> CreateUserAsync(UserPostDto userDto)
        {
            return await _api.PostAsync<UserDto>(
                $"api/users/create?userType={userDto.UserType}",
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

        public async Task<ChangePasswordResponseDto> ChangePasswordAsync(
      string userId,
      string newPassword)
        {
            Debug.WriteLine($"[UserService] Change password for {userId}");

            return await _api.PatchAsync<ChangePasswordResponseDto>(
                $"api/users/change-password?id={userId}",
                $"\"{newPassword}\""
            );
        }



        // ===================== TOGGLE STATUS =====================
        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            var result = await _api.PostAsync<object>(
    $"api/users/toggle-active?id={userId}",
    null
);

            return result != null;

        }

        // ===================== DELETE =====================
        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                await _api.DeleteAsync($"api/users/delete?id={userId}");
                return true; // لو وصل هنا، يبقى نجح
            }
            catch
            {
                return false; // لو حصل خطأ
            }
        }


        // ===================== GET BY ID =====================
        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            return await _api.GetAsync<UserDto>(
                $"api/users/get-user?id={id}"
            );
        }

        // ===================== GET CURRENT USER =====================
        // في دالة GetCurrentUserAsync
        // في دالة GetCurrentUserAsync
        public async Task<CurrentUserDto?> GetCurrentUserAsync()
        {
            try
            {
                Debug.WriteLine($"[UserService] [{DateTime.Now:HH:mm:ss}] طلب بيانات المستخدم الحالي...");

                // أولاً: جرب باستخدام ApiResponse
                Debug.WriteLine($"[UserService] محاولة جلب البيانات كـ ApiResponse<CurrentUserDto>");
                try
                {
                    var apiResponse = await _api.GetAsync<ApiResponse<CurrentUserDto>>("api/users/current-user");
                    if (apiResponse != null && apiResponse.StatusCode == 200 && apiResponse.Data != null)
                    {
                        Debug.WriteLine($"[UserService] نجح كـ ApiResponse");
                        Debug.WriteLine($"[UserService] StatusCode: {apiResponse.StatusCode}");
                        Debug.WriteLine($"[UserService] Message: {apiResponse.Message}");
                        return apiResponse.Data;
                    }
                }
                catch (Exception ex1)
                {
                    Debug.WriteLine($"[UserService] فشل كـ ApiResponse: {ex1.Message}");
                }

                // ثانياً: جرب مباشرة
                Debug.WriteLine($"[UserService] محاولة جلب البيانات مباشرة كـ CurrentUserDto");
                try
                {
                    var directResponse = await _api.GetAsync<CurrentUserDto>("api/users/current-user");
                    if (directResponse != null)
                    {
                        Debug.WriteLine($"[UserService] نجح كـ CurrentUserDto مباشرة");
                        Debug.WriteLine($"[UserService] ID: {directResponse.Id}");
                        Debug.WriteLine($"[UserService] Fullname: {directResponse.Fullname}");
                        Debug.WriteLine($"[UserService] StatusCode في البيانات: {directResponse.StatusCode}");
                        return directResponse;
                    }
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"[UserService] فشل كـ CurrentUserDto مباشرة: {ex2.Message}");
                }

                Debug.WriteLine($"[UserService] جميع المحاولات فشلت");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UserService] خطأ غير متوقع: {ex.Message}");
                Debug.WriteLine($"[UserService] Stack Trace: {ex.StackTrace}");
                return null;
            }
        }






    }
}
