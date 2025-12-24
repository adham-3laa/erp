using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using erp.DTOs;
using erp.Services;

namespace erp.Services.Category;

public sealed class CategoryService
{
    private readonly ApiClient _api;

    public CategoryService(ApiClient api)
        => _api = api ?? throw new ArgumentNullException(nameof(api));

    // GET /api/Categories/GetAllCategories
    public Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default)
        => _api.GetAsync<List<CategoryDto>>("/api/Categories/GetAllCategories", ct);

    // GET /api/Categories/GetCategoryById?id=...
    public Task<CategoryDto> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Category id is required.", nameof(id));

        return _api.GetAsync<CategoryDto>(
            $"/api/Categories/GetCategoryById?id={Uri.EscapeDataString(id)}", ct);
    }

    // POST /api/Categories/CreateCategory
    public Task<CategoryDto> CreateAsync(CreateCategoryRequest req, CancellationToken ct = default)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        return _api.PostAsync<CategoryDto>("/api/Categories/CreateCategory", req, ct);
    }

    // PUT /api/Categories/UpdateCategory
    public Task<CategoryDto> UpdateAsync(UpdateCategoryRequest req, CancellationToken ct = default)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        return _api.PutAsync<CategoryDto>("/api/Categories/UpdateCategory", req, ct);
    }

    // DELETE /api/Categories/DeleteCategory?id=...
    public Task DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Category id is required.", nameof(id));

        return _api.DeleteAsync(
            $"/api/Categories/DeleteCategory?id={Uri.EscapeDataString(id)}", ct);
    }
}
