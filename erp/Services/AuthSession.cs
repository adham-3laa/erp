namespace erp.Services;

public interface IAuthSession
{
    string? AccessToken { get; set; }
    string? RefreshToken { get; set; }
    DateTime? TokenExpiry { get; set; }

    bool IsAuthenticated { get; }
    void Clear();
}

public sealed class AuthSession : IAuthSession
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiry { get; set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        TokenExpiry = null;
    }
}
