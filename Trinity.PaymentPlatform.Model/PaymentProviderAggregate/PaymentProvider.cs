using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Model.PaymentProviderAggregate;

public class PaymentProvider:Entity<int>,IAggregateRoot
{
    public virtual string Name { get; protected set; }
    public virtual string ApiUrl { get; protected set; }
    public virtual bool IsActive { get; protected set; }
    public virtual string MerchantId { get; protected set; }
    public virtual string CustomerKey { get; protected set; }
    public virtual string CustomerSecret { get; protected set; }
    public virtual string CallbackUrl { get; protected set; }

    protected PaymentProvider() { }

    public PaymentProvider(string name, string apiUrl, bool isActive, string merchantId, string customerKey, string customerSecret, string callbackUrl)
    {
        Name = name;
        ApiUrl = apiUrl;
        IsActive = isActive;
        MerchantId = merchantId;
        CustomerKey = customerKey;
        CustomerSecret = customerSecret;
        CallbackUrl = callbackUrl;
    }
}