using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using erp.DTOs;

namespace erp.Services;

public sealed class CategoryService
{
    private readonly ApiClient _api;
    public CategoryService(ApiClient api) => _api = api;

    public Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default) =>
        _api.GetAsync<List<CategoryDto>>("api/Categories/GetAllCategories", ct);

    public Task<CategoryDto> GetByIdAsync(string id, CancellationToken ct = default) =>
        _api.GetAsync<CategoryDto>(
            $"api/Categories/GetCategoryById?id={Uri.EscapeDataString(id)}", ct);

    public Task<CategoryDto> CreateAsync(CreateCategoryRequest req, CancellationToken ct = default) =>
        _api.PostAsync<CategoryDto>("api/Categories/CreateCategory", req, ct);

    public Task<CategoryDto> UpdateAsync(UpdateCategoryRequest req, CancellationToken ct = default) =>
        _api.PutAsync<CategoryDto>("api/Categories/UpdateCategory", req, ct);

    public Task DeleteAsync(string id, CancellationToken ct = default) =>
        _api.DeleteAsync(
            $"api/Categories/DeleteCategory?id={Uri.EscapeDataString(id)}", ct);
}
