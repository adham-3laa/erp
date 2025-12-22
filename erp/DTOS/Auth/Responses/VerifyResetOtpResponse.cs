namespace erp.DTOS.Auth.Responses;

public sealed record VerifyResetOtpResponse(
    string ResetToken
);

//ده endpoint /password/verifyotp بيرجع ResetToken