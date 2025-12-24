
namespace erp.DTOS.Auth.Requests
{
    public sealed record ResetPasswordRequest(string ResetToken, string NewPassword);

}
