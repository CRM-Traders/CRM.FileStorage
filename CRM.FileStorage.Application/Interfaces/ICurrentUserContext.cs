namespace CRM.FileStorage.Application.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? Email { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}