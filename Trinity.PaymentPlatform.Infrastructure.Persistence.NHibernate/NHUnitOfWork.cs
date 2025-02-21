using System.Data;
using NHibernate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate;

 public class NHUnitOfWork : IUnitOfWork
    {
        private ISession? _session;
        private ITransaction? _transaction;

        private readonly INHibernateHelper _nhibernateHelper;

        public ISession Session => _session;

        public NHUnitOfWork(INHibernateHelper nhibernateHelper)
        {
            _nhibernateHelper = nhibernateHelper;
            OpenSession();
        }

        private void OpenSession()
        {
            if (_session is not { IsConnected: true })
            {
                if (_session != null)
                    _session.Dispose();
                _session = _nhibernateHelper.OpenSession();
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction is not { IsActive: true })
            {
                _transaction?.Dispose();
                //todo: maybe add transaction isolation level to function parameters

                _transaction = _session.BeginTransaction(isolationLevel);
            }
        }

        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
        }

        public async Task CommitAsync()
        {
            try
            {
                await _transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await _transaction.RollbackAsync();
                throw;
            }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (_session != null)
            {
                _session.Close();
                _session.Dispose();
                _session = null;
            }
        }

        public void Rollback()
        {
            if(_transaction is { IsActive: true })
                _transaction.Rollback();
        }

        public async Task RollbackAsync()
        {
            if(_transaction is { IsActive: true })
                await _transaction.RollbackAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_session != null) await CastAndDispose(_session);
            if (_transaction != null) await CastAndDispose(_transaction);

            return;

            static async ValueTask CastAndDispose(IDisposable resource)
            {
                if (resource is IAsyncDisposable resourceAsyncDisposable)
                    await resourceAsyncDisposable.DisposeAsync();
                else
                    resource.Dispose();
            }
        }
    }