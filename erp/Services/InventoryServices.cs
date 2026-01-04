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
                BaseAddress = new Uri("http://be-positive.runasp.net")
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
     $"/api/Inventory/ListOfProductsWithCategoryName?supplierName={Uri.EscapeDataString(supplierId)}";


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

            if (string.IsNullOrWhiteSpace(product.ProductId))
                throw new Exception("ProductId مفقود");

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new Exception("اسم المنتج مطلوب");

            if (string.IsNullOrWhiteSpace(product.Category))
                throw new Exception("اسم الصنف مطلوب");

            if (product.SalePrice <= 0)
                throw new Exception("سعر البيع غير صالح");

            if (product.Quantity <= 0)
                throw new Exception("الكمية غير صالحة");

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
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

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


    }
}
