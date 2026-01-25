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
                BaseAddress = new Uri("http://be-positive.runasp.net/")
            };
        }

        public async Task<PrintableInvoiceDto?> BuildPrintableInvoiceAsync(
            UserDto user,
            InvoiceResponseDto invoice)
        {
<<<<<<< HEAD
=======
            if (invoice == null)
                return null;

            // =====================================================
            // ============== SUPPLIER INVOICE =====================
            // =====================================================
            // في SupplierInvoice الـ API بيرجع items داخل الفاتورة نفسها (مش OrderId)
            // لو مش موجودة، بنجيبها من endpoint GetSupplierInviceProductsByInvoicCode
            if (invoice.InvoiceTypeParsed == Enums.InvoiceType.SupplierInvoice)
            {
                var printable = new PrintableInvoiceDto
                {
                    InvoiceId = invoice.Id,
                    InvoiceCode = invoice.code, // ✅ Sequential invoice number for printing
                    InvoiceDate = invoice.GeneratedDate,
                    CustomerName = invoice.SupplierName ?? invoice.RecipientName ?? user.Fullname,
                    CustomerEmail = user.Email,
                    OrderId = invoice.code.ToString(), // ✅ Use Invoice Code as reference for Supplier Invoices
                    PaidAmount = invoice.PaidAmount,
                    RemainingAmount = invoice.RemainingAmount
                };

                // ✅ لو Items موجودة في الفاتورة مباشرة
                if (invoice.Items != null && invoice.Items.Count > 0)
                {
                    foreach (var it in invoice.Items)
                    {
                        if (it == null) continue;

                        printable.Items.Add(new PrintableInvoiceItemDto
                        {
                            ProductName = it.ProductName ?? "-",
                            Quantity = it.Quantity,
                            UnitPrice = it.UnitPrice,
                            CategoryName = it.CategoryName ?? "غير محدد"
                        });
                    }
                }
                else
                {
                    // ✅ جلب المنتجات من API باستخدام invoice code
                    try
                    {
                        // Use injected service instead of creating new one
                        var supplierProducts = await _invoiceService.GetSupplierInvoiceProductsAsync(invoice.code);

                        // Load inventory & categories to resolve category names
                        // Renamed to avoid scope conflict with outer variables
                        var allInventoryProducts = await _inventoryService.GetAllProductsAsync();
                        var allCategoriesMap = await GetCategoriesMapAsync();

                        foreach (var sp in supplierProducts)
                        {
                            if (sp == null) continue;

                            var product = allInventoryProducts
                                .FirstOrDefault(p => string.Equals(p.ProductId?.Trim(), sp.ProductId?.Trim(), StringComparison.OrdinalIgnoreCase));

                            var categoryName = ResolveCategoryName(product?.Category, allCategoriesMap);

                            printable.Items.Add(new PrintableInvoiceItemDto
                            {
                                ProductName = sp.ProductName ?? "-",
                                Quantity = (int)sp.Quantity,
                                UnitPrice = sp.BuyPrice,
                                CategoryName = categoryName
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but allow printing partial invoice if needed
                        System.Diagnostics.Debug.WriteLine($"Failed to load supplier products: {ex.Message}");
                    }
                }

                // Return printable even if items are empty, so we can at least print the header
                // But the caller expects items generally. Let's return what we have.
                return printable;
            }

            // =====================================================
            // ============== CUSTOMER INVOICE =====================
            // =====================================================
>>>>>>> ad1f622d97b67f8b3e45d4015a285558ad57c332
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

<<<<<<< HEAD
            var printable = new PrintableInvoiceDto
=======
            // Use OrderCode if available for better readability, otherwise fallback to GUID
            string displayOrderId = (invoice.OrderCode.HasValue && invoice.OrderCode.Value > 0) 
                ? invoice.OrderCode.Value.ToString() 
                : orderId;

            var printableCustomer = new PrintableInvoiceDto
>>>>>>> ad1f622d97b67f8b3e45d4015a285558ad57c332
            {
                InvoiceId = invoice.Id,
                InvoiceDate = invoice.GeneratedDate,
                CustomerName = user.Fullname,
                CustomerEmail = user.Email,
<<<<<<< HEAD
                OrderId = invoice.OrderId.Value.ToString(),
=======
                OrderId = displayOrderId,
>>>>>>> ad1f622d97b67f8b3e45d4015a285558ad57c332
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
