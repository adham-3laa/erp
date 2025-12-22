

namespace erp.DTOS.Auth.Requests
{
    public sealed record VerifyResetOtpRequest(string Email, string Otp);

}
