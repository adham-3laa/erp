using erp.DTOS;
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
        public async Task<StockMovementReportDto?> GetStockMovementAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name is required");

            return await _api.GetAsync<StockMovementReportDto>(
                $"api/Reports/stock-movement?producName={Uri.EscapeDataString(productName)}"
            );
        }

        public async Task<CommissionReportResponseDto?> GetCommissionsAsync(string salesRepId)
        {
            if (string.IsNullOrWhiteSpace(salesRepId))
                throw new ArgumentException("SalesRepId is required");

            return await _api.GetAsync<CommissionReportResponseDto>(
                $"api/Reports/commissions?salesRepId={salesRepId}"
            );
        }

    }
}
