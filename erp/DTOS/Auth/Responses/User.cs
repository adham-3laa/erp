namespace erp.DTOS.Auth.Responses;

public sealed record User(
    string Id,
    string Username,
    string Email,
    string UserType,
    string[] Roles
);
