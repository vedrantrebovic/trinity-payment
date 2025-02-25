using FluentResults;

namespace Trinity.PaymentPlatform.Model.Contracts;

public interface ITransactionInitiator
{
    static string Name;
    Task<Result<long>> CreatePayIn(IPayInTransactionInitiationParams parameters);
    Task<Result<long>> CreatePayout(IPayoutTransactionInitiationParams parameters);
}

/// <summary>
/// Marker interface for pay in transaction parameters //todo: maybe add common properties here
/// </summary>
public interface IPayInTransactionInitiationParams { }
/// <summary>
/// Marker interface for payout transaction parameters //todo: maybe add common properties here
/// </summary>
public interface IPayoutTransactionInitiationParams { }