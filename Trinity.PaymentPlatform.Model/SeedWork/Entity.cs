namespace Trinity.PaymentPlatform.Model.SeedWork
{
    public abstract class Entity<TId>: IAuditable
    {
        int? _requestedHashCode;

        public virtual TId Id { get; protected set; }
        
        protected Entity(TId id)
        {
            Id = id;
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ModifiedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        protected Entity()
        {
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ModifiedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public virtual bool IsTransient()
        {
            return Id.Equals(default(TId));
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                _requestedHashCode ??= this.Id.GetHashCode() ^ 31;

                return _requestedHashCode.Value;
            }
            else
                return base.GetHashCode();
        }

        #region Auditing

        public virtual long CreatedAt { get; protected set; }
        public virtual long ModifiedAt { get; protected set; }
        public virtual void UpdateModifiedAt(long timestamp)
        {
            ModifiedAt = timestamp;
        }

        public virtual void UpdateCreatedAt(long timestamp)
        {
            CreatedAt = timestamp;
        }
        
        #endregion

        #region Equality

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<TId> item)
                return false;

            if (ReferenceEquals(this, item))
                return true;

            if (GetType() != item.GetType())
                return false;

            if (item.IsTransient() || IsTransient())
                return false;
            return Id.Equals(item.Id);
        }

        public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        {
            if (Equals(left, null))
                return Equals(right, null);

            return left.Equals(right);
        }

        public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        {
            return !(left == right);
        }

        #endregion
    }
}
