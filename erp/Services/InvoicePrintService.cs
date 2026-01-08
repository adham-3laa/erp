using erp.DTOS;
using erp.DTOS.InvoicesDTOS;
using erp.DTOS.OrderDTOs;
using EduGate.Models;                 // ✅ Product
using erp.DTOS.Inventory.Responses;   // ✅ ApiResponse<T>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace erp.Services
{
    public class InvoicePrintService
    {
        private readonly OrdersService _ordersService;
        private readonly InventoryService _inventoryService;

        // ✅ Categories client
        private readonly HttpClient _categoriesClient;

        public InvoicePrintService(
            OrdersService ordersService,
            InventoryService inventoryService)
        {
            _ordersService = ordersService;
            _inventoryService = inventoryService;

            _categoriesClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };
        }

        public async Task<PrintableInvoiceDto?> BuildPrintableInvoiceAsync(
            UserDto user,
            InvoiceResponseDto invoice)
        {
            if (invoice.OrderId == null)
                return null;

            // 1️⃣ Get order items (اولًا من Orders endpoint، ولو فشل نجرب Returns)
            List<OrderItemDto> orderItems;
            try
            {
                orderItems = await _ordersService
                    .GetOrderItemsAsync(invoice.OrderId.Value.ToString());
            }
            catch
            {
                orderItems = await _ordersService
                    .GetOrderItemsByOrderIdAsync(invoice.OrderId.Value.ToString());
            }

            if (orderItems == null || orderItems.Count == 0)
                return null;

            // 2️⃣ Get inventory products (فيها السعر + categoryId)
            var inventoryProducts = await _inventoryService.GetAllProductsAsync();

            // 3️⃣ Get categories map (categoryId -> categoryName)
            var categoriesMap = await GetCategoriesMapAsync();

            var printable = new PrintableInvoiceDto
            {
                InvoiceId = invoice.Id,
                InvoiceDate = invoice.GeneratedDate,
                CustomerName = user.Fullname,
                CustomerEmail = user.Email,
                OrderId = invoice.OrderId.Value.ToString(),
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount
            };

            // 4️⃣ Map OrderItem -> Inventory Product -> Category Name
            foreach (var orderItem in orderItems)
            {
                if (orderItem == null || string.IsNullOrWhiteSpace(orderItem.ProductId))
                    continue;

                var product = inventoryProducts
                    .FirstOrDefault(p =>
                        string.Equals(p.ProductId?.Trim(), orderItem.ProductId?.Trim(),
                            StringComparison.OrdinalIgnoreCase));

                // لو المنتج مش موجود في المخزون هنكمل بس باللي عندنا من الطلب
                var productName = product?.Name ?? orderItem.ProductName ?? "-";

                // ✅ السعر: لو سعر الـ order item = 0 ناخد من الـ inventory
                var unitPrice =
                    (orderItem.Price > 0)
                        ? orderItem.Price
                        : (product != null ? product.SalePrice : 0);

                // ✅ اسم الفئة: product.Category غالبًا فيها categoryId (GUID)
                var categoryName = ResolveCategoryName(product?.Category, categoriesMap);

                printable.Items.Add(new PrintableInvoiceItemDto
                {
                    ProductName = productName,
                    Quantity = orderItem.Quantity,
                    UnitPrice = unitPrice,
                    CategoryName = categoryName
                });
            }

            // لو مفيش items بعد الربط
            if (!printable.Items.Any())
                return null;

            return printable;
        }

        // =========================
        // Helpers
        // =========================

        private async Task<Dictionary<string, string>> GetCategoriesMapAsync()
        {
            // Attach JWT
            if (!string.IsNullOrWhiteSpace(TokenStore.Token))
            {
                _categoriesClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }

            var response = await _categoriesClient.GetAsync("api/Categories/GetAllCategories");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<DTOS.Inventory.Responses.ApiResponse<List<CategoryDto>>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var list = apiResponse?.value ?? new List<CategoryDto>();

            // map: id -> name (case-insensitive keys)
            return list
                .Where(c => !string.IsNullOrWhiteSpace(c.id) && !string.IsNullOrWhiteSpace(c.name))
                .GroupBy(c => c.id.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().name, StringComparer.OrdinalIgnoreCase);
        }

        private string ResolveCategoryName(string? categoryValue, Dictionary<string, string> categoriesMap)
        {
            // categoryValue غالبًا = categoryId (GUID) من InventoryService.GetAllProductsAsync
            if (string.IsNullOrWhiteSpace(categoryValue))
                return "غير محدد";

            var trimmed = categoryValue.Trim();

            // لو Guid => نجيب الاسم من map
            if (Guid.TryParse(trimmed, out _))
            {
                if (categoriesMap.TryGetValue(trimmed, out var catName) && !string.IsNullOrWhiteSpace(catName))
                    return catName;

                return "غير محدد";
            }

            // لو مش Guid يبقى غالبًا الاسم نفسه
            return trimmed;
        }

        // DTO بسيط للفئات (GetAllCategories بيرجع value فيها عناصر فيها id/name)
        private class CategoryDto
        {
            public string id { get; set; } = "";
            public string name { get; set; } = "";
        }
    }
}
