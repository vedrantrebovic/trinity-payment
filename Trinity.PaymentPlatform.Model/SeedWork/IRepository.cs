namespace Trinity.PaymentPlatform.Model.SeedWork;

public interface IRepository<T, in TId> where T : IAggregateRoot
{
    Task<T> GetAsync(TId id);
    Task SaveAsync(T obj);
    Task UpdateAsync(T obj);
    Task DeleteAsync(T obj);
    
}