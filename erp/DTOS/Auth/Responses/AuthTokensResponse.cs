namespace erp.DTOS.Auth.Responses;

public sealed record AuthTokensResponse(
    string Token,
    string RefreshToken,
    DateTime TokenExpiry
);
//ده اللي هنخزّنه بعد login