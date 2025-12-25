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

    // GET /api/Categories/GetAllCategories  => { ..., "value": [ ... ] }
    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<ApiListResponse<List<CategoryDto>>>(
            "/api/Categories/GetAllCategories", ct);

        return res.Value ?? new List<CategoryDto>();
    }

    // GET /api/Categories/GetCategoryById?id=...  (نتوقع يرجّع Category داخل value أو object مشابه)
    public async Task<CategoryDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Category id is required.", nameof(id));

        // في أغلب APIs عندكم بيرجع Wrapper، لو طلع شكل مختلف هظبطه فورًا
        var res = await _api.GetAsync<ApiListResponse<CategoryDto>>(
            $"/api/Categories/GetCategoryById?id={Uri.EscapeDataString(id)}", ct);

        return res.Value;
    }

    // POST /api/Categories/CreateCategory  => { ..., "categoryid": "..." }
    public async Task<string> CreateAsync(CreateCategoryRequest req, CancellationToken ct = default)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var res = await _api.PostAsync<CreateCategoryApiResponse>(
            "/api/Categories/CreateCategory", req, ct);

        if (string.IsNullOrWhiteSpace(res.CategoryId))
            throw new InvalidOperationException(res.Message ?? "CreateCategory returned empty categoryid.");

        return res.CategoryId;
    }

    // PUT /api/Categories/UpdateCategory
    // (مبدئيًا نخليه يرجع CategoryDto? بنفس Wrapper، لو endpoint بتاع Update بيرجع شكل تاني ابعته واظبطه)
    public async Task<CategoryDto?> UpdateAsync(UpdateCategoryRequest req, CancellationToken ct = default)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var res = await _api.PutAsync<ApiListResponse<CategoryDto>>(
            "/api/Categories/UpdateCategory", req, ct);

        return res.Value;
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
