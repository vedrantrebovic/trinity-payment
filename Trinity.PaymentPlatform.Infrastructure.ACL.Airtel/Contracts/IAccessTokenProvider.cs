using FluentResults;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;

public interface IAccessTokenProvider
{
    string? GetToken();
    Task<Result> ObtainToken();


}