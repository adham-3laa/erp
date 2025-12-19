using erp.DTOS;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace erp.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://be-positive.runasp.net/")
            };
        }

        // ===================== CREATE USER =====================
        public async Task<UserDto> CreateUserAsync(UserPostDto userDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/users/create?userType={userDto.UserType}",
                    userDto
                );

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // ===================== UPDATE USER =====================
        public async Task<UserDto> UpdateUserAsync(string id, UserUpdateDto updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"/api/users/update?id={id}",
                    updateDto
                );

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }


        public async Task<UserResponseDto> GetUsersAsync(
            string search = null,
            string userType = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            try
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

                var response = await _httpClient.GetAsync($"api/users/list?{query}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<UserResponseDto>();
            }
            catch (Exception ex)
            {
                return new UserResponseDto
                {
                    Users = new System.Collections.Generic.List<UserDto>(),
                    TotalCount = 0,
                    FilteredCount = 0
                };
            }
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PatchAsync(
                    $"api/users/toggle-active?id={userId}", null);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"api/users/delete?id={userId}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users/current-user");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/get-user?id={id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}