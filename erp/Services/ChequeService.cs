using erp.DTOS;
using erp.DTOS.Cheques;
using erp.DTOS.Inventory.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace erp.Services
{
    /// <summary>
    /// Custom exception for service-level errors with Arabic messages
    /// </summary>
    public class ServiceException : Exception
    {
        public string ArabicMessage { get; }

        public ServiceException(string arabicMessage, Exception? innerException = null) 
            : base(arabicMessage, innerException)
        {
            ArabicMessage = arabicMessage;
        }
    }

    public class ChequeService
    {
        private readonly ApiClient _api;

        public ChequeService(ApiClient api)
        {
            _api = api;
        }

        // ================== Get All Cheques ==================
        public async Task<List<ChequeDto>> GetAllChequesAsync(string searchName = "", string direction = "", string status = "")
        {
            try
            {
                var query = "/api/Cheques/list?";
                if (!string.IsNullOrWhiteSpace(searchName)) query += $"searchName={Uri.EscapeDataString(searchName)}&";
                if (!string.IsNullOrWhiteSpace(direction)) query += $"direction={Uri.EscapeDataString(direction)}&";
                if (!string.IsNullOrWhiteSpace(status)) query += $"status={Uri.EscapeDataString(status)}&";

                var response = await _api.GetAsync<ApiResponse<List<ChequeDto>>>(query);
                return response?.value ?? new List<ChequeDto>();
            }
            catch (Exception ex)
            {
                // Log the technical error for developers
                ErrorHandlingService.LogError(ex, "ChequeService.GetAllChequesAsync");
                
                // Throw with Arabic message for the ViewModel to handle
                throw new ServiceException(ErrorHandlingService.Messages.ChequeLoadError, ex);
            }
        }

        // ================== Add Cheque ==================
        public async Task<(bool Success, string? ErrorMessage)> AddChequeAsync(AddChequeRequest request)
        {
            try
            {
                var result = await _api.PostWithStatusAsync<object>("/api/Cheques/add", request);
                
                if (result.StatusCode == System.Net.HttpStatusCode.OK || 
                    result.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    ErrorHandlingService.LogInfo($"Cheque added successfully: {request.CheckNumber}");
                    return (true, null);
                }
                
                // Successful HTTP call but unexpected status
                var message = ErrorHandlingService.GetMessageForStatusCode(result.StatusCode);
                return (false, message);
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, $"ChequeService.AddChequeAsync - CheckNumber: {request.CheckNumber}");
                return (false, ErrorHandlingService.GetUserFriendlyMessage(ex, ErrorHandlingService.Messages.ChequeAddError));
            }
        }

        // ================== Update Status ==================
        public async Task<(bool Success, string? ErrorMessage)> UpdateChequeStatusAsync(int cheqCode, string newStatus)
        {
            try
            {
                var url = $"/api/Cheques/update-status?cheqCode={cheqCode}&newStatus={Uri.EscapeDataString(newStatus)}";
                var result = await _api.PutWithStatusAsync<object>(url, new { });
                
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ErrorHandlingService.LogInfo($"Cheque status updated: Code={cheqCode}, NewStatus={newStatus}");
                    return (true, null);
                }
                
                var message = ErrorHandlingService.GetMessageForStatusCode(result.StatusCode);
                return (false, message);
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, $"ChequeService.UpdateChequeStatusAsync - Code: {cheqCode}, Status: {newStatus}");
                return (false, ErrorHandlingService.GetUserFriendlyMessage(ex, ErrorHandlingService.Messages.ChequeStatusUpdateError));
            }
        }

        // ================== Update Cheque ==================
        public async Task<(bool Success, string? ErrorMessage)> UpdateChequeAsync(UpdateChequeRequest request)
        {
            try
            {
                var body = new
                {
                    code = request.Code,
                    checknumber = request.CheckNumber,
                    amount = request.Amount,
                    duedate = request.DueDate,
                    bankname = request.BankName,
                    isincoming = request.IsIncoming,
                    relatedname = request.RelatedName,
                    notes = request.Notes
                };

                var result = await _api.PutWithStatusAsync<object>("/api/Cheques/update", body);
                
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ErrorHandlingService.LogInfo($"Cheque updated successfully: Code={request.Code}");
                    return (true, null);
                }
                
                var message = ErrorHandlingService.GetMessageForStatusCode(result.StatusCode);
                return (false, message);
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, $"ChequeService.UpdateChequeAsync - Code: {request.Code}");
                return (false, ErrorHandlingService.GetUserFriendlyMessage(ex, ErrorHandlingService.Messages.ChequeUpdateError));
            }
        }
    }
}

