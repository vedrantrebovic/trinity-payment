using MediatR;
using OneOf;
using OneOf.Types;
using Trinity.PaymentPlatform.Model.SharedKernel;

namespace Trinity.PaymentPlatform.Application.Shared.Contracts;

public interface IBaseRequestHandler<in TCommand, TResult> : IRequestHandler<TCommand, OneOf<TResult, None, List<IDomainError>, Exception>>
    where TCommand : IRequest<OneOf<TResult, None, List<IDomainError>, Exception>>
{

}