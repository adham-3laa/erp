using erp.ViewModels;
using erp;
using erp.DTOS.Inventory.Requests;
using erp.DTOS.Inventory.Responses;
using erp.DTOS.InventoryCheck.Requests;
using erp.DTOS.InventoryCheck.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using EduGate.Models;

namespace erp.Services
{
    public class InventoryService
    {
        private readonly HttpClient _client;

        public InventoryService()
        {
            _client = ApiClient.CreateHttpClient();
        }

        // ================== ✅ NEW: Lightweight DTO for printing ==================
        // ده ملوش أي تأثير على باقي النظام، مجرد DTO مساعد للطباعة
        public class InventoryProductInfo
        {
            public string ProductId { get; set; } = "";
            public int Code { get; set; }
            public string ProductName { get; set; } = "";
            public decimal SalePrice { get; set; }
            public decimal BuyPrice { get; set; }
            public int Quantity { get; set; }
            public string SKU { get; set; } = "N/A";
            public string Description { get; set; } = "";
            public string? CategoryId { get; set; }
            public string? CategoryName { get; set; }
        }

        // ================== ✅ NEW: Get All Products Info (saleprice + categoryname) ==================
        // دي اللي هنستخدمها علشان نطلع السعر + اسم الفئة
        public async Task<List<InventoryProductInfo>> GetAllProductsInfoAsync(int skip = 0, int take = 1000)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response = await _client.GetAsync($"/api/Inventory/GetAllproducts?skip={skip}&take={take}");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var apiResponse =
                JsonSerializer.Deserialize<ApiResponse<List<InventoryItemResponse>>>(
                    jsonString,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            if (string.IsNullOrWhiteSpace(jsonString))
                return new List<InventoryProductInfo>();

            using var doc = JsonDocument.Parse(jsonString);

            // شكل الاستجابة: { statusCode, message, traceId, value: [ ... ] }
            if (!doc.RootElement.TryGetProperty("value", out var valueEl) || valueEl.ValueKind != JsonValueKind.Array)
                return new List<InventoryProductInfo>();

            var list = new List<InventoryProductInfo>();

            foreach (var p in valueEl.EnumerateArray())
            {
                // Helpers
                string GetString(string name)
                    => p.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() ?? "" : "";

                string? GetNullableString(string name)
                    => p.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;

                decimal GetDecimal(string name)
                {
                    if (!p.TryGetProperty(name, out var el)) return 0;
                    if (el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out var d)) return d;
                    return 0;
                }

                int GetInt(string name)
                {
                    if (!p.TryGetProperty(name, out var el)) return 0;
                    if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var i)) return i;
                    return 0;
                }

                list.Add(new InventoryProductInfo
                {
                    ProductId = GetString("productid"),
                    Code = GetInt("code"),
                    ProductName = GetString("productname"),
                    // ✅ API بيرجع saleprice / buyprice
                    SalePrice = GetDecimal("saleprice"),
                    BuyPrice = GetDecimal("buyprice"),
                    Quantity = GetInt("quantity"),
                    SKU = GetString("sku"),
                    Description = GetString("description"),
                    CategoryId = GetNullableString("categoryid"),
                    CategoryName = GetNullableString("categoryname")
                });
            }

            return list;
        }

        // ================== Get All Products ==================
        // ✅ نفس التوقيع زي ما هو (علشان باقي السيستم)
        // ✅ بس هنخليه يعتمد على الميثود الجديدة علشان الأسعار تبقى صح
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var infos = await GetAllProductsInfoAsync(skip: 0, take: 1000);

            if (infos == null)
                return new List<Product>();

            return infos.Select(p => new Product
            {
                ProductId = p.ProductId,
                code = p.Code,
                Name = p.ProductName ?? "",

                SalePrice = Convert.ToInt32(p.SalePrice),
                BuyPrice = Convert.ToInt32(p.BuyPrice),

                Quantity = p.Quantity,  // السماح بالكمية 0 (نفاد المخزون)
                SKU = string.IsNullOrWhiteSpace(p.SKU) ? "N/A" : p.SKU,

                // ✅ عرض اسم الصنف بدل الكود
                Category = !string.IsNullOrWhiteSpace(p.CategoryName)
        ? p.CategoryName
        : "غير محدد",

                Description = p.Description ?? "",
                Supplier = ""
            }).ToList();

        }



        // ================== Delete ==================
        public async Task<bool> DeleteProductAsync(string id)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response =
                await _client.DeleteAsync($"/api/Inventory/products?id={id}");

            return response.IsSuccessStatusCode;
        }

        // ================== Add Single Product ==================
        public async Task AddProductAsync(object body)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response =
                await _client.PostAsJsonAsync("/api/Inventory/products", body);

            response.EnsureSuccessStatusCode();
        }

        // ================== Add List Of Products ==================
        public async Task AddProductsWithCategoryNameAsync(
         List<Product> products,
         string supplierId)
        {
            if (string.IsNullOrWhiteSpace(supplierId))
                throw new Exception("SupplierId مطلوب");

            if (products == null || products.Count == 0)
                throw new Exception("قائمة المنتجات فارغة");

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var body = products.Select((p, index) =>
            {
                if (p == null)
                    throw new Exception($"المنتج رقم {index + 1} فارغ");

                if (string.IsNullOrWhiteSpace(p.Name))
                    throw new Exception($"اسم المنتج رقم {index + 1} مطلوب");

                if (string.IsNullOrWhiteSpace(p.Category))
                    throw new Exception($"اسم الصنف للمنتج رقم {index + 1} مطلوب");

                if (p.SalePrice <= 0 || p.BuyPrice <= 0 || p.Quantity <= 0)
                    throw new Exception($"أسعار / كمية غير صحيحة في المنتج رقم {index + 1}");

                return new CreateProductWithCategoryNameRequest
                {
                    productname = p.Name.Trim(),
                    saleprice = p.SalePrice,
                    buyprice = p.BuyPrice,
                    quantity = p.Quantity,
                    sku = string.IsNullOrWhiteSpace(p.SKU) ? null : p.SKU.Trim(),
                    description = p.Description?.Trim() ?? "",
                    categoryname = p.Category.Trim()
                };
            }).ToList();

            var url = $"/api/Inventory/ListOfProductsWithCategoryName?supplierName={Uri.EscapeDataString(supplierId)}";

            var response = await _client.PostAsJsonAsync(url, body);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string msg;
                try
                {
                    var jsonDoc = JsonDocument.Parse(content);
                    msg = jsonDoc.RootElement.GetProperty("message").GetString() ?? content;
                }
                catch
                {
                    msg = content; // لو مش JSON
                }

                throw new Exception($"API Error: {msg}");
            }
        }



        // ================== Update Product ==================
        public async Task UpdateProductAsync(Product product)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            if (string.IsNullOrWhiteSpace(product.ProductId))
                throw new Exception("ProductId مفقود");

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new Exception("اسم المنتج مطلوب");

            if (string.IsNullOrWhiteSpace(product.Category))
                throw new Exception("اسم الصنف مطلوب");

            if (product.SalePrice <= 0)
                throw new Exception("سعر البيع غير صالح");

            if (product.Quantity < 0)
                throw new Exception("الكمية غير صالحة (يجب أن تكون 0 أو أكثر)");

            if (string.IsNullOrWhiteSpace(product.SKU))
                product.SKU = "N/A";

            var request = new UpdateProductWithCategoryNameRequest
            {
                productid = product.ProductId,
                productname = product.Name.Trim(),
                sellprice = product.SalePrice,
                quantity = product.Quantity,
                sku = product.SKU.Trim(),
                description = product.Description ?? "",
                categoryname = product.Category.Trim()
            };

            var response = await _client.PutAsJsonAsync(
                "/api/Inventory/productsWithCategoryName",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }




        // ================== Inventory Check ==================
        public async Task<InventoryAdjustmentResponse> AdjustInventoryByNameAsync(
      string productName,
      int actualQuantity,
      bool updateStock)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", TokenStore.Token);

            var body = new
            {
                productname = productName,
                actualquantity = actualQuantity
            };

            var response = await _client.PostAsJsonAsync(
                $"/api/InventoryCheck/Adjust?UpdateStock={updateStock.ToString().ToLower()}",
                body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<InventoryAdjustmentResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }



        // ================== Search Product By Name ==================
        public async Task<List<Product>> SearchProductsByNameAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return new List<Product>();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response = await _client.GetAsync(
                $"/api/Inventory/products?ProductName={Uri.EscapeDataString(productName)}");

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var apiResponse =
                JsonSerializer.Deserialize<ApiResponse<List<InventoryItemResponse>>>(
                    jsonString,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            return apiResponse?.value.Select(p => new Product
            {
                ProductId = p.productid,
                Name = p.productname,
                SalePrice = (int)p.saleprice,
                BuyPrice = (int)p.buyprice,
                Quantity = p.quantity,
                SKU = p.sku,
                Category = p.categoryname
            }).ToList() ?? new List<Product>();
        }

        public async Task<int> StockInProductsAsync(
        string supplierName,
        List<StockInItemRequest> items)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
                throw new Exception("اسم المورد مطلوب");

            if (items == null || items.Count == 0)
                throw new Exception("قائمة المنتجات فارغة");

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response = await _client.PostAsJsonAsync(
                $"/api/Inventory/Listproducts/stock/in?supplierName={Uri.EscapeDataString(supplierName)}",
                items);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ApiResponse<int>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.value ?? 0;
        }

        public class ProductLookupDto
        {
            public string ProductId { get; set; } = "";
            public string ProductName { get; set; } = "";
            public decimal SalePrice { get; set; }
            public string? CategoryId { get; set; }
            public string? CategoryName { get; set; }
        }

        // ✅✅ FIX: ما بقيناش نعتمد على InventoryItemResponse (عشان categoryname مش موجودة)
        // هنستخدم GetAllProductsInfoAsync اللي بتقرأ JSON مباشرة (وفيه categoryname)
        public class AutocompleteProductDto
        {
            public int Code { get; set; }
            public string Name { get; set; } = "";
        }

        public async Task<List<AutocompleteProductDto>> GetAutocompleteProductsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<AutocompleteProductDto>();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response = await _client.GetAsync($"/api/Inventory/autocomplete?term={Uri.EscapeDataString(term)}");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<AutocompleteProductDto>>>(
                jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse?.value ?? new List<AutocompleteProductDto>();
        }

        public async Task<List<ProductLookupDto>> GetAllProductsLookupAsync()
        {
            var infos = await GetAllProductsInfoAsync(skip: 0, take: 1000);

            return infos.Select(p => new ProductLookupDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                SalePrice = p.SalePrice,
                CategoryId = p.CategoryId,
                CategoryName = p.CategoryName
            }).ToList();
        }
    }
}
