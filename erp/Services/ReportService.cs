using erp.DTOS;
using erp.DTOS.Reports;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace erp.Services
{
    public class ReportService
    {
        private readonly ApiClient _api;

        public ReportService(ApiClient api)
        {
            _api = api;
        }

        public async Task<SalesReportDto?> GetSalesReportAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            // 🔒 تأكيد إن FromDate أصغر من ToDate
            if (fromDate > toDate)
                throw new ArgumentException("FromDate must be earlier than ToDate");

            // ✅ تحويل لـ UTC + ISO 8601 (المطلوب من الـ API)
            var from = fromDate
                .ToUniversalTime()
                .ToString("o", CultureInfo.InvariantCulture);

            var to = toDate
                .ToUniversalTime()
                .ToString("o", CultureInfo.InvariantCulture);

            return await _api.GetAsync<SalesReportDto>(
                $"api/Reports/sales?fromDate={from}&toDate={to}"
            );
        }

        public async Task<FullSalesReportDto?> GetFullSalesReportAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            // 🔒 تأكيد إن FromDate أصغر من ToDate
            if (fromDate > toDate)
                throw new ArgumentException("FromDate must be earlier than ToDate");

            // ✅ تحويل لـ UTC + ISO 8601 (المطلوب من الـ API)
            var from = fromDate
                .ToUniversalTime()
                .ToString("o", CultureInfo.InvariantCulture);

            var to = toDate
                .ToUniversalTime()
                .ToString("o", CultureInfo.InvariantCulture);

            return await _api.GetAsync<FullSalesReportDto>(
                $"api/Reports/Full-sales-report?fromDate={from}&toDate={to}"
            );
        }
        public async Task<StockMovementReportDto?> GetStockMovementAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name is required");

            return await _api.GetAsync<StockMovementReportDto>(
                $"api/Reports/stock-movement?producName={Uri.EscapeDataString(productName)}"
            );
        }

        public async Task<CommissionReportResponseDto?> GetCommissionsAsync(string salesRepName)
        {
            if (string.IsNullOrWhiteSpace(salesRepName))
                throw new ArgumentException("SalesRepName is required");

            return await _api.GetAsync<CommissionReportResponseDto>(
                $"api/Reports/commissions?salesRepName={Uri.EscapeDataString(salesRepName)}"
            );
        }

        public async Task<CustomerReportDto?> GetCustomerReportAsync(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name is required");

            return await _api.GetAsync<CustomerReportDto>(
                $"api/Reports/customer-report?customerName={Uri.EscapeDataString(customerName)}"
            );
        }

        public async Task<SalesRepReportDto?> GetSalesRepReportAsync(string salesRepName)
        {
            if (string.IsNullOrWhiteSpace(salesRepName))
                throw new ArgumentException("Sales representative name is required");

            return await _api.GetAsync<SalesRepReportDto>(
                $"api/Reports/sales-rep-report?salesRepName={Uri.EscapeDataString(salesRepName)}"
            );
        }

        public async Task<SupplierReportDto?> GetSupplierReportAsync(string supplierName)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
                throw new ArgumentException("Supplier name is required");

            return await _api.GetAsync<SupplierReportDto>(
                $"api/Reports/supplier-report?supplierName={Uri.EscapeDataString(supplierName)}"
            );
        }

        public async Task<SuppliersListResponseDto?> GetSuppliersAsync()
        {
            return await _api.GetAsync<SuppliersListResponseDto>("api/Supplier/suppliers");
        }

        public async Task<System.Collections.Generic.List<erp.DTOS.Inventory.ProductSuggestionDto>> GetProductSuggestionsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new System.Collections.Generic.List<erp.DTOS.Inventory.ProductSuggestionDto>();

            var response = await _api.GetAsync<erp.DTOS.ApiResponseForReturn<System.Collections.Generic.List<erp.DTOS.Inventory.ProductSuggestionDto>>>(
                $"api/Inventory/autocomplete?term={Uri.EscapeDataString(term)}"
            );

            return response?.Value ?? new System.Collections.Generic.List<erp.DTOS.Inventory.ProductSuggestionDto>();
        }

        public async Task<System.Collections.Generic.List<erp.DTOS.Orders.SalesRepSuggestionDto>> GetSalesRepSuggestionsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new System.Collections.Generic.List<erp.DTOS.Orders.SalesRepSuggestionDto>();

            var response = await _api.GetAsync<erp.DTOS.ApiResponseForReturn<System.Collections.Generic.List<erp.DTOS.Orders.SalesRepSuggestionDto>>>(
                $"api/Orders/SalesRep/autocomplete?term={Uri.EscapeDataString(term)}"
            );

            return response?.Value ?? new System.Collections.Generic.List<erp.DTOS.Orders.SalesRepSuggestionDto>();
        }

        public async Task<System.Collections.Generic.List<erp.DTOS.Reports.CustomerSuggestionDto>> GetCustomerSuggestionsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new System.Collections.Generic.List<erp.DTOS.Reports.CustomerSuggestionDto>();

            var response = await _api.GetAsync<erp.DTOS.ApiResponseForReturn<System.Collections.Generic.List<erp.DTOS.Reports.CustomerSuggestionDto>>>(
                $"api/Orders/customers/autocomplete?term={Uri.EscapeDataString(term)}"
            );

            return response?.Value ?? new System.Collections.Generic.List<erp.DTOS.Reports.CustomerSuggestionDto>();
        }

        public async Task<DualRoleReportDto?> GetDualRoleReportAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required");

            return await _api.GetAsync<DualRoleReportDto>(
                $"api/Reports/dual-role-report?name={Uri.EscapeDataString(name)}"
            );
        }
    }
}
