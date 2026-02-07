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
        private readonly InvoiceService _invoiceService;

        private readonly HttpClient _categoriesClient;

        public InvoicePrintService(
            OrdersService ordersService,
            InventoryService inventoryService,
            InvoiceService invoiceService)
        {
            _ordersService = ordersService;
            _inventoryService = inventoryService;
            _invoiceService = invoiceService;

            _categoriesClient = ApiClient.CreateHttpClient();
        }

        public async Task<PrintableInvoiceDto?> BuildPrintableInvoiceAsync(
            UserDto user,
            InvoiceResponseDto invoice)
        {
            if (invoice == null)
                return null;

            // =====================================================
            // ============== SUPPLIER INVOICE =====================
            // =====================================================
            // في SupplierInvoice الـ API بيرجع items داخل الفاتورة نفسها (مش OrderId)
            // لو مش موجودة، بنجيبها من endpoint GetSupplierInviceProductsByInvoicCode
            // =====================================================
            // ============== SUPPLIER INVOICES (Supply & Return) =====================
            // =====================================================
            // Handle both SupplierInvoice and SupplierReturnInvoice
            if (invoice.InvoiceTypeParsed == Enums.InvoiceType.SupplierInvoice || 
                invoice.InvoiceTypeParsed == Enums.InvoiceType.SupplierReturnInvoice)
            {
                var title = "فاتورة مورد";
                if (invoice.InvoiceTypeParsed == Enums.InvoiceType.SupplierReturnInvoice)
                    title = "فاتورة مرتجع مورد";

                var printable = new PrintableInvoiceDto
                {
                    InvoiceId = invoice.Id,
                    InvoiceTypeTitle = title,
                    InvoiceCode = invoice.code, // ✅ Sequential invoice number for printing
                    InvoiceDate = invoice.GeneratedDate,
                    CustomerName = invoice.SupplierName ?? invoice.RecipientName ?? user.Fullname,
                    CustomerEmail = user.Email,
                    OrderId = invoice.code.ToString(), // ✅ Use Invoice Code as reference
                    PaidAmount = invoice.PaidAmount,
                    RemainingAmount = invoice.RemainingAmount
                };

                // Adjust title/header based on type if needed inside PrintableInvoiceDto 
                // (though PrintableInvoiceDto currently doesn't carry Type info, the name usually comes from page title)
                
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
                        if (invoice.InvoiceTypeParsed == Enums.InvoiceType.SupplierReturnInvoice)
                        {
                            // ==================== SUPPLIER RETURN ====================
                            // Load using MANDATORY endpoint for returns
                            var returnProducts = await _invoiceService.GetSupplierReturnInvoiceProductsAsync(invoice.code);

                            // Load inventory for mapping product details
                            var allInventoryProductsMap = (await _inventoryService.GetAllProductsAsync())
                                .Where(p => !string.IsNullOrEmpty(p.ProductId))
                                .ToDictionary(p => p.ProductId.Trim(), StringComparer.OrdinalIgnoreCase);
                            
                            var returnCategoriesMap = await GetCategoriesMapAsync();

                            foreach (var rp in returnProducts)
                            {
                                if (rp == null) continue;
                                
                                var categoryName = "غير محدد";
                                if (allInventoryProductsMap.TryGetValue(rp.ProductId?.Trim() ?? "", out var invProduct))
                                {
                                    categoryName = ResolveCategoryName(invProduct.Category, returnCategoriesMap);
                                }

                                printable.Items.Add(new PrintableInvoiceItemDto
                                {
                                    ProductName = rp.ProductName ?? "-",
                                    Quantity = (int)rp.Quantity,
                                    UnitPrice = rp.BuyPrice, 
                                    CategoryName = categoryName
                                });
                            }
                        }
                        else
                        {
                            // ==================== SUPPLIER INVOICE ====================
                            var supplierProducts = await _invoiceService.GetSupplierInvoiceProductsAsync(invoice.code);

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
                    }
                    catch (Exception ex)
                    {
                        // Log error but allow printing partial invoice if needed
                        System.Diagnostics.Debug.WriteLine($"Failed to load supplier products: {ex.Message}");
                    }
                }

                // Return printable even if items are empty
                return printable;
            }

            // =====================================================
            // ============== CUSTOMER INVOICE =====================
            // =====================================================
            if (invoice.OrderId == null)
                return null;

            var orderId = invoice.OrderId.Value.ToString();

            // 1️⃣ Get order items (بنفس منطق شاشة التفاصيل: نجرب Returns first ثم Orders)
            List<OrderItemDto> orderItems = null;
            
            try
            {
                // محاولة أولى: استخدام OrderId (GUID) من Returns endpoint
                orderItems = await _ordersService.GetOrderItemsByOrderIdAsync(orderId);
            }
            catch (Exception ex1) when (ex1.Message.Contains("404") || ex1.Message.Contains("Not Found"))
            {
                // لو فشل بـ OrderId (GUID)، نجرب Orders endpoint
                try
                {
                    orderItems = await _ordersService.GetOrderItemsAsync(orderId);
                }
                catch (Exception ex2) when (ex2.Message.Contains("404") || ex2.Message.Contains("Not Found"))
                {
                    // لو فشل كمان، نحاول نستخدم orderCode
                    // نجيب الطلب من قائمة الطلبات المعتمدة للحصول على code
                    var orders = await _ordersService.GetApprovedOrdersAsync();
                    var order = orders.FirstOrDefault(o => o.id == orderId);
                    
                    if (order != null && order.code > 0)
                    {
                        // استخدام orderCode للبحث من Returns endpoint
                        var returnsService = new ReturnsService(App.Api);
                        var orderItemsForReturn = await returnsService.GetOrderItemsByOrderIdAsync(order.code.ToString());
                        
                        // تحويل OrderItemForReturnDto إلى OrderItemDto
                        orderItems = orderItemsForReturn.Select(item => new OrderItemDto
                        {
                            ProductId = item.Productid,
                            ProductName = item.Productname,
                            Quantity = item.Quantity,
                            Price = item.Unitprice
                        }).ToList();
                    }
                }
            }
            catch
            {
                // أي خطأ تاني، نجرب Orders endpoint
                try
                {
                    orderItems = await _ordersService.GetOrderItemsAsync(orderId);
                }
                catch
                {
                    orderItems = null;
                }
            }

            if (orderItems == null || orderItems.Count == 0)
                return null;

            // 2️⃣ Get inventory products (فيها السعر + categoryId)
            var inventoryProducts = await _inventoryService.GetAllProductsAsync();

            // 3️⃣ Get categories map (categoryId -> categoryName)
            var categoriesMap = await GetCategoriesMapAsync();

            var displayOrderId = (invoice.OrderCode.HasValue && invoice.OrderCode.Value > 0) 
                ? invoice.OrderCode.Value.ToString() 
                : orderId;

            // Determine Title
            var customerTitle = "فاتورة مبيعات"; // Default
            if (invoice.InvoiceTypeParsed == Enums.InvoiceType.ReturnInvoice || 
                invoice.InvoiceTypeParsed == Enums.InvoiceType.SupplierReturnInvoice) // Should be ReturnInvoice usually
            {
                customerTitle = "فاتورة مرتجع";
            }
            else if (invoice.InvoiceTypeParsed == Enums.InvoiceType.CommissionInvoice)
            {
                customerTitle = "فاتورة عمولة";
            }

            var printableCustomer = new PrintableInvoiceDto
            {
                InvoiceId = invoice.Id,
                InvoiceTypeTitle = customerTitle,
                InvoiceType = invoice.InvoiceTypeParsed,
                InvoiceCode = invoice.code, // ✅ Sequential invoice number for printing
                InvoiceDate = invoice.GeneratedDate,
                CustomerName = invoice.RecipientName ?? user.Fullname,
                CustomerEmail = user.Email,
                OrderId = displayOrderId,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount,
                // ✅ For commission invoices, set the commission amount (from invoice.Amount)
                CommissionAmount = invoice.InvoiceTypeParsed == Enums.InvoiceType.CommissionInvoice 
                    ? invoice.Amount 
                    : null
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

                printableCustomer.Items.Add(new PrintableInvoiceItemDto
                {
                    ProductName = productName,
                    Quantity = orderItem.Quantity,
                    UnitPrice = unitPrice,
                    CategoryName = categoryName
                });
            }

            return printableCustomer.Items.Any() ? printableCustomer : null;
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
