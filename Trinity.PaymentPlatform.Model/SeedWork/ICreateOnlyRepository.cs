namespace Trinity.PaymentPlatform.Model.SeedWork;

public interface ICreateOnlyRepository<T, in TId> where T : IAggregateRoot
{
    Task<T> GetAsync(TId id);
    Task SaveAsync(T obj);
}