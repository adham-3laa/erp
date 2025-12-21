using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOs;

public sealed class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("password")]
    public string Password { get; set; } = "";
}

public sealed class LoginResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errorcode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("auth")]
    public AuthSection? Auth { get; set; }

    [JsonPropertyName("verification")]
    public VerificationSection? Verification { get; set; }

    [JsonPropertyName("user")]
    public UserSection? User { get; set; }
}

public sealed class AuthSection
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("refreshtoken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("tokenexpiry")]
    public DateTime? TokenExpiry { get; set; }
}

public sealed class VerificationSection
{
    [JsonPropertyName("requiresotpverification")]
    public bool RequiresOtpVerification { get; set; }

    [JsonPropertyName("emailconfirmed")]
    public bool EmailConfirmed { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public sealed class UserSection
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("username")]
    public string? UserName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("usertype")]
    public string? UserType { get; set; }

    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }
}

public sealed class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}
