using NHibernate.Event;
using NHibernate.Persister.Entity;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Utils;

public class AuditEventListener : IPreUpdateEventListener, IPreInsertEventListener
    {
        public bool OnPreInsert(PreInsertEvent @event)
        {
            var audit = @event.Entity as IAuditable;
            if (audit == null)
                return false;
            
            var time = DateTimeOffset.UtcNow;
            Set(@event.Persister, @event.State, nameof(audit.ModifiedAt), time.ToUnixTimeMilliseconds());
            Set(@event.Persister, @event.State, nameof(audit.CreatedAt), time.ToUnixTimeMilliseconds());
            audit.UpdateModifiedAt(time.ToUnixTimeMilliseconds());
            audit.UpdateCreatedAt(time.ToUnixTimeMilliseconds());
            return false;
        }

        public Task<bool> OnPreInsertAsync(PreInsertEvent @event, CancellationToken cancellationToken)
        {
            var audit = @event.Entity as IAuditable;
            if (audit == null)
                return Task.FromResult(false);
            
            var time = DateTimeOffset.UtcNow;
            Set(@event.Persister, @event.State, nameof(audit.ModifiedAt), time.ToUnixTimeMilliseconds());
            Set(@event.Persister, @event.State, nameof(audit.CreatedAt), time.ToUnixTimeMilliseconds());
            audit.UpdateModifiedAt(time.ToUnixTimeMilliseconds());
            audit.UpdateCreatedAt(time.ToUnixTimeMilliseconds());
            return Task.FromResult(false);
        }

        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            var audit = @event.Entity as IAuditable;
            if (audit == null)
                return false;
            
            var time = DateTimeOffset.UtcNow;
            Set(@event.Persister, @event.State, nameof(audit.ModifiedAt), time.ToUnixTimeMilliseconds());
            audit.UpdateModifiedAt(time.ToUnixTimeMilliseconds());

            return false;
        }

        public Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
        {
            var audit = @event.Entity as IAuditable;
            if (audit == null)
                return Task.FromResult(false);
            
            var time = DateTimeOffset.UtcNow;
            Set(@event.Persister, @event.State, nameof(audit.ModifiedAt), time.ToUnixTimeMilliseconds());
            audit.UpdateModifiedAt(time.ToUnixTimeMilliseconds());

            return Task.FromResult(false);
        }
        
        private void Set(IEntityPersister persister, object[] state
            , string propertyName, object value)
        {
            var index = Array.IndexOf(persister.PropertyNames, propertyName);
            if (index == -1)
                return;
            state[index] = value;
        }
    }