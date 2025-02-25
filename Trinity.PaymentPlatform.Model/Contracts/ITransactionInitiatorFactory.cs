namespace Trinity.PaymentPlatform.Model.Contracts;

public interface ITransactionInitiatorFactory
{
    ITransactionInitiator GetTransactionInitiator(int id);
}

public interface IPayInTransactionInitiationParamsConverter
{
    IPayInTransactionInitiationParams? Convert(int id, IPayInTransactionInputParams input);
}

public interface IPayInTransactionInputParams{}

public interface IPayoutTransactionInitiationParamsConverter
{
    IPayoutTransactionInitiationParams? Convert(int id, IPayoutTransactionInputParams input);
}
public interface IPayoutTransactionInputParams { }
