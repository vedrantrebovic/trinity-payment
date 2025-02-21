using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Model.SharedKernel;

public class Address:ValueObject
{
    public virtual string? Street { get; protected set; }
    public virtual string? HouseNumber { get; protected set; }
    public virtual string? ZipCode { get; protected set; }
    public virtual string? City { get; protected set; }
    public virtual string? Country { get; protected set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return HouseNumber;
        yield return ZipCode;
        yield return City;
        yield return Country;
    }

    protected Address(){}

    public Address(string? street, string? houseNumber, string? zipCode, string? city, string? country)
    {
        Street = street;
        HouseNumber = houseNumber;
        ZipCode = zipCode;
        City = city;
        Country = country;
    }
}