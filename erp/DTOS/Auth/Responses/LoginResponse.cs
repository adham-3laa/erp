namespace erp.DTOS.Auth.Responses;

public sealed record LoginResponse(
    int StatusCode,
    string Message,
    string? TraceId,
    bool Success,
    string? ErrorCode,
    AuthTokensResponse Auth,
    LoginVerificationResponse Verification,
    User User
);

//ده يغطي الحالتين:
//login بيرجع tokens مباشرة
// login بيقولك “OTP required”