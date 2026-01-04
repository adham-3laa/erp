using EduGate.Models;
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

namespace erp.Services
{
    public class InventoryService
    {
        private readonly HttpClient _client;

        public InventoryService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://be-positive.runasp.net/")
            };
        }

        // ================== ✅ NEW: Lightweight DTO for printing ==================
        // ده ملوش أي تأثير على باقي النظام، مجرد DTO مساعد للطباعة
        public class InventoryProductInfo
        {
            public string ProductId { get; set; } = "";
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

            return infos.Select(p => new Product
            {
                ProductId = p.ProductId,
                Name = p.ProductName,

                // ✅ تحويل آمن من decimal لـ int
                SalePrice = Convert.ToInt32(p.SalePrice),

                // كان زمان بيرجع categoryid (guid) — هنخليه زي ما هو علشان باقي السيستم
                Category = p.CategoryId ?? "",

                BuyPrice = Convert.ToInt32(p.BuyPrice),
                Quantity = p.Quantity > 0 ? p.Quantity : 1,
                SKU = string.IsNullOrWhiteSpace(p.SKU) ? "N/A" : p.SKU,
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

        // ================== Add Single Product (قديم) ==================
        public async Task AddProductAsync(object body)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response =
                await _client.PostAsJsonAsync("/api/Inventory/products", body);

            response.EnsureSuccessStatusCode();
        }

        // ================== Add List Of Products (NEW API) ==================
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

                if (string.IsNullOrWhiteSpace(p.SKU))
                    throw new Exception($"SKU للمنتج رقم {index + 1} مطلوب");

                if (p.SalePrice <= 0 || p.BuyPrice <= 0 || p.Quantity <= 0)
                    throw new Exception($"أسعار / كمية غير صحيحة في المنتج رقم {index + 1}");

                return new CreateProductWithCategoryNameRequest
                {
                    productname = p.Name.Trim(),
                    saleprice = p.SalePrice,
                    buyprice = p.BuyPrice,
                    quantity = p.Quantity,
                    sku = p.SKU.Trim(),
                    description = p.Description?.Trim() ?? "",
                    categoryname = p.Category.Trim()
                };
            }).ToList();

            var url =
                $"/api/Inventory/ListOfProductsWithCategoryName?SupplierId={supplierId}";

            var response = await _client.PostAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        // ================== Update Product ==================
        public async Task UpdateProductAsync(Product product)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            // ===== Validation =====
            if (string.IsNullOrWhiteSpace(product.ProductId))
                throw new Exception("ProductId مفقود");

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new Exception("اسم المنتج مطلوب");

            if (string.IsNullOrWhiteSpace(product.Category))
                product.Category = "default";

            if (product.SalePrice <= 0)
                throw new Exception("سعر البيع غير صالح");

            if (product.BuyPrice <= 0)
                product.BuyPrice = product.SalePrice;

            if (product.Quantity <= 0)
                product.Quantity = 1;

            if (string.IsNullOrWhiteSpace(product.SKU))
                product.SKU = "N/A";

            var request = new UpdateProductRequest
            {
                productid = product.ProductId,
                productname = product.Name,
                sellprice = product.SalePrice,
                buyprice = product.BuyPrice,
                quantity = product.Quantity,
                sku = product.SKU,
                description = product.Description ?? "",
                categoryid = product.Category
            };

            var response =
                await _client.PutAsJsonAsync("/api/Inventory/products", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        public async Task<InventoryAdjustmentResponse> AdjustInventoryAsync(
            string productId,
            int actualQuantity)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var body = new
            {
                productid = productId,
                actualquantity = actualQuantity
            };

            var response = await _client.PostAsJsonAsync(
                "/api/InventoryCheck/Adjust",
                body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<InventoryAdjustmentResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result!;
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
