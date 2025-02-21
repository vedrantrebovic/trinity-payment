namespace Trinity.PaymentPlatform.Model.SeedWork;

public interface IAuditable
{
    long CreatedAt { get; }
    long ModifiedAt { get; }
    void UpdateModifiedAt(long timestamp);
    void UpdateCreatedAt(long timestamp);
}