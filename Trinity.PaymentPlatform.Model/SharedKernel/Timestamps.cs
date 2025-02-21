using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Model.SharedKernel;

public class Timestamps:ValueObject
{
    public virtual long CreatedAt { get; protected set; }
    public virtual long ReceivedAt { get; protected set; }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CreatedAt;
        yield return ReceivedAt;
    }

    protected Timestamps() { }

    private Timestamps(long created, long received)
    {
        CreatedAt = created;
        ReceivedAt = received;
    }

    public static Timestamps Create(long created, long received)
    {
        return new Timestamps(created, received);
    }

    public static Timestamps UtcNow => Timestamps.Create(DateTime.UtcNow.ToUnixTimestampMilliseconds(),
        DateTime.UtcNow.ToUnixTimestampMilliseconds());
}