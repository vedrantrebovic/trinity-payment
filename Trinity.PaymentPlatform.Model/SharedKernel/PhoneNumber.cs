using System.Text.RegularExpressions;
using FluentResults;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Model.SharedKernel;

public class PhoneNumber:ValueObject
{
    private const string REGEX_PATTERN = @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}";
    private const string NumbersOnlyRegexPattern = "^[0-9]+$";
        
    public virtual string Number { get; protected set; }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
    }
        
    protected PhoneNumber (){}

    private PhoneNumber(string number)
    {
        Number = number;
    }

    public static Result<PhoneNumber> Create(string number)
    {
        if (!Regex.IsMatch(number, NumbersOnlyRegexPattern))
            return Result.Fail("err.invalid_phone_number");
        return Result.Ok(new PhoneNumber(number));
    }
}