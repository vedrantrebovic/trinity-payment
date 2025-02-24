using FluentResults;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;

public interface IAccessTokenProvider
{
    string? GetToken();
    Task<Result> ObtainToken();
}