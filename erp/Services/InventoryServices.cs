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

namespace EduGate.Services
{
    public class InventoryService
    {
        private readonly HttpClient _client;

        public InventoryService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://warhouse.runasp.net/")
            };
        }

        // ================== Get All Products ==================
        public async Task<List<Product>> GetAllProductsAsync()
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response =
                await _client.GetAsync("/api/Inventory/GetAllproducts?skip=0&take=100");

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse =
                JsonSerializer.Deserialize<ApiResponse<List<InventoryItemResponse>>>(jsonString, options);

            return apiResponse?.value.Select(p => new Product
            {
                ProductId = p.productid,
                Name = p.productname,
                SalePrice = (int)p.sellprice,
                Category = p.categoryid ?? "",

                // قيم افتراضية (لأن الـ API لا يعيدها)
                BuyPrice = (int)p.sellprice,
                Quantity = 1,
                SKU = "N/A",
                Description = "",
                Supplier = ""
            }).ToList() ?? new List<Product>();
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

    }
}
