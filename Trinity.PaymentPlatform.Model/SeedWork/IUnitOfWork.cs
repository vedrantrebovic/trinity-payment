

using System.Data;

namespace Trinity.PaymentPlatform.Model.SeedWork;

public interface IUnitOfWork : IAsyncDisposable
{
    void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    void Commit();
    void Rollback();
    Task CommitAsync();
    Task RollbackAsync();
}