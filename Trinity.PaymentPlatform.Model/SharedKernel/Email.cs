using System.Net.Mail;
using FluentResults;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Model.SharedKernel;

public class Email:ValueObject
{
    private const int MAX_LENGTH = 254;
    private const string REGEX_PATTERN =
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$"; 
    public virtual string Address { get; protected set; }
        
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
    }
    
    protected Email(){}
        
    private Email(string address)
    {
        Address = address;
    }

    public static Result<Email> Create(string address)
    {
        if (!IsValidMail(address)) return Result.Fail("err.invalid_email");
        return Result.Ok(new Email(address));
    }

    private static bool IsValidMail(string address)
    {
        return MailAddress.TryCreate(address, out _);
    }
}