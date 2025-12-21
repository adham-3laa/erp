using EduGate.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
                BaseAddress = new Uri("https://your-api-base-url") // ضع هنا رابط الـ API الصحيح
            };
        }

        // 1️⃣ جلب كل المنتجات
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _client.GetFromJsonAsync<List<Product>>("/api/Inventory/GetAllproducts");
        }

        // 2️⃣ جلب منتج واحد حسب المعرف
        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _client.GetFromJsonAsync<Product>($"/api/Inventory/products?id={id}");
        }

        // 3️⃣ إنشاء منتج جديد
        public async Task<bool> CreateProductAsync(Product product)
        {
            var response = await _client.PostAsJsonAsync("/api/Inventory/products", product);
            return response.IsSuccessStatusCode;
        }

        // 4️⃣ تحديث منتج موجود
        public async Task<bool> UpdateProductAsync(Product product)
        {
            var response = await _client.PutAsJsonAsync("/api/Inventory/products", product);
            return response.IsSuccessStatusCode;
        }

        // 5️⃣ حذف منتج حسب المعرف
        public async Task<bool> DeleteProductAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/Inventory/products?id={id}");
            return response.IsSuccessStatusCode;
        }

        // 6️⃣ زيادة كمية المخزون
        public async Task<bool> UpdateStockInAsync(int productId, int quantityToAdd)
        {
            var data = new { ProductId = productId, Quantity = quantityToAdd };
            var response = await _client.PostAsJsonAsync("/api/Inventory/products/stock/in", data);
            return response.IsSuccessStatusCode;
        }

        internal async Task AddProductAsync(object value)
        {
            throw new NotImplementedException();
        }
    }
}
