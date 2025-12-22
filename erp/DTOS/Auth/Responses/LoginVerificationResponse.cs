namespace erp.DTOS.Auth.Responses;

public sealed record LoginVerificationResponse(
    bool RequiresOtpVerification,
    bool EmailConfirmed,
    string Email
);
