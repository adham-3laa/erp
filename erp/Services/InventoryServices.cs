using EduGate.Models;
using erp;
using erp.DTOS.Inventory.Requests;
using erp.DTOS.Inventory.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace EduGate.Services
{
    public class InventoryService
    {
        private readonly HttpClient _client;

        public InventoryService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://be-positive.runasp.net")
            };
        }

        // ================== Get All Products (زي ما هو + تحسين) ==================
        public async Task<List<Product>> GetAllProductsAsync()
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response =
                await _client.GetAsync("/api/Inventory/GetAllproducts?skip=0&take=100");

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var apiResponse =
                JsonSerializer.Deserialize<ApiResponse<List<InventoryItemResponse>>>(jsonString, options);

            return apiResponse?.value.Select(p => new Product
            {
                ProductId = p.productid,
                Name = p.productname,
                SalePrice = (int)p.sellprice,
                Category = p.categoryid ?? "",

                // ⚠️ القيم دي الـ API مش بيرجعها
                // نحط قيم افتراضية علشان التعديل مايفشلش
                BuyPrice = (int)p.sellprice,
                Quantity = 1,
                SKU = "N/A",
                Description = "",
                Supplier = ""
            }).ToList() ?? new List<Product>();
        }

        // ================== Delete (بدون تغيير) ==================
        public async Task<bool> DeleteProductAsync(string id)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response =
                await _client.DeleteAsync($"/api/Inventory/products?id={id}");

            return response.IsSuccessStatusCode;
        }

        // ================== ADD LIST OF PRODUCTS (API الجديد) ==================
        public async Task AddProductsWithCategoryNameAsync(List<Product> products)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var body = products.Select(p => new CreateProductWithCategoryNameRequest
            {
                productname = p.Name,
                saleprice = p.SalePrice,
                buyprice = p.BuyPrice,
                quantity = p.Quantity,
                sku = p.SKU,
                description = p.Description ?? "",
                categoryname = p.Category,

                // ⭐ REQUIRED
                supplierid = p.Supplier
            }).ToList();

            var response = await _client.PostAsJsonAsync(
                "/api/Inventory/ListOfProductsWithCategoryName",
                body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }


        // ================== Add (بدون تغيير) ==================
        public async Task AddProductAsync(object body)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            await _client.PostAsJsonAsync("/api/Inventory/products", body);
        }

        // ================== Update (FIX 400 بدون حذف أي حاجة) ==================
        public async Task UpdateProductAsync(Product product)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            // ====== حماية من 400 Bad Request ======

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
                saleprice = product.SalePrice,
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
    }
}
