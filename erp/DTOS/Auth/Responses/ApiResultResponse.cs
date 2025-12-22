namespace erp.DTOS.Auth.Responses;

public sealed record ApiResultResponse(
    bool Success,
    string? Message
);
//ده مناسب لأي endpoint بيرجع success/message