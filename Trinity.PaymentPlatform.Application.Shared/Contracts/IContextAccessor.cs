namespace Trinity.PaymentPlatform.Application.Shared.Contracts;

public interface IContextAccessor
{
    string? GetIpAddress();
    string? GetUserAgent();
}