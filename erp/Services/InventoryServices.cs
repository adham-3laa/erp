using EduGate.Models;
using erp;
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

        public async Task<List<Product>> GetAllProductsAsync()
        {
            _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response = await _client.GetAsync("/api/Inventory/GetAllproducts?skip=0&take=100");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<InventoryItemResponse>>>(jsonString, options);

            return apiResponse?.value.Select(p => new Product
            {
                ProductId = p.productid,
                Name = p.productname,
                SalePrice = (int)p.sellprice, // لو انت عايز تخليه int للـ DataGrid
                Category = p.categoryid ?? ""
            }).ToList() ?? new List<Product>();

        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            var response = await _client.DeleteAsync($"/api/Inventory/products?id={id}");
            return response.IsSuccessStatusCode;
        }

        public async Task AddProductAsync(object body)
        {
            _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);

            await _client.PostAsJsonAsync("/api/Inventory/products", body);
        }

        public async Task UpdateProductAsync(Product product)
        {

            _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.Token);
            await _client.PutAsJsonAsync("/api/Inventory/products", product);
        }
    }
}
