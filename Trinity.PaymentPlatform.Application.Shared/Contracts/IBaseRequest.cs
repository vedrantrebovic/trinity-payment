using MediatR;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Application.Shared.Contracts;

public interface IBaseRequest<T> : IRequest<OneOf<T, None, List<IDomainError>, Exception>> { }