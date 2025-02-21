using FluentResults;
using Trinity.PaymentPlatform.Model.SeedWork;
using Trinity.PaymentPlatform.Model.Util;

namespace Trinity.PaymentPlatform.Model.SharedKernel;

public class Money:ValueObject
{
    public static Money Zero(string currencyCode) => new (0m, currencyCode);
    public static Money Zero(Money from) => new(0m, from.CurrencyCode);
    public static Money Negative(Money amount) => new (-1 * amount.Amount, amount.CurrencyCode);
    public virtual decimal Amount { get; protected set; }
    public virtual string CurrencyCode { get; protected set; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return CurrencyCode;
    }
    
    protected Money(){}

    public Money(decimal amount, string currencyCode)
    {
        if (!IsValidCurrencyCode(currencyCode))
            throw new InvalidOperationException(ErrorMessageFormatter.Error("invalid_currency_code"));
        Amount = amount;
        CurrencyCode = currencyCode;
    }

    public Money(Money money)
    {
        Amount = money.Amount;
        CurrencyCode = money.CurrencyCode;
    }

    public static Money Create(decimal amount, Money currencySource) => new Money(amount, currencySource.CurrencyCode);

    public static Money CreateEuro(decimal amount) => new(amount, "EUR");

    public static Result<Money> Create(decimal amount, string currencyCode)
    {
        try
        {
            var money = new Money(amount, currencyCode);
            return Result.Ok(money);
        }
        catch (Exception)
        {
            return ErrorMessageFormatter.FailWithError("invalid_currency_code");
        }
    }

    public override string ToString()
    {
        return $"{Amount:N2}{CurrencyCode}";
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.CurrencyCode != right.CurrencyCode)
            throw new InvalidOperationException(ErrorMessageFormatter.Error("currency_code_mismatch"));
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.CurrencyCode != right.CurrencyCode)
            throw new InvalidOperationException(ErrorMessageFormatter.Error("currency_code_mismatch"));
        return left.Amount < right.Amount;
    }
    
    public static bool operator >=(Money left, Money right)
    {
        if (left.CurrencyCode != right.CurrencyCode)
            throw new InvalidOperationException(ErrorMessageFormatter.Error("currency_code_mismatch"));
        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left.CurrencyCode != right.CurrencyCode)
            throw new InvalidOperationException(ErrorMessageFormatter.Error("currency_code_mismatch"));
        return left.Amount <= right.Amount;
    }

    public static bool operator ==(Money left, Money right)
    {
        if (left is null && right is null) return true;
        
        return left.Equals(right);
        // if (left.CurrencyCode != right.CurrencyCode)
        //     throw new InvalidOperationException("err.currency_code_mismatch");
        //
        // return left.Amount == right.Amount;
    }

    public static bool operator !=(Money left, Money right)
    {
         return !(left == right);
    }

    public static Money operator +(Money left, Money right)
    {
        if (left.CurrencyCode != right.CurrencyCode)
            throw new InvalidOperationException(ErrorMessageFormatter.Error("currency_code_mismatch"));
        return new Money(left.Amount + right.Amount, left.CurrencyCode);
    }
    
    public static Money operator - (Money left, Money right)
    {
        if (left.CurrencyCode != right.CurrencyCode)
            throw new InvalidOperationException(ErrorMessageFormatter.Error("currency_code_mismatch"));
        return new Money(left.Amount - right.Amount, left.CurrencyCode);
    }

    public static Money operator *(Money left, decimal multiplier)
    {
        return new Money(left.Amount * multiplier, left.CurrencyCode);
    }
    
    public static Money operator *(decimal multiplier, Money right)
    {
        return new Money(right.Amount * multiplier, right.CurrencyCode);
    }
    
    public static Money operator /(Money left, decimal divisor)
    {
        return new Money(left.Amount / divisor, left.CurrencyCode);
    }
    
    public static Money operator /(decimal divisor, Money right)
    {
        return new Money(right.Amount / divisor, right.CurrencyCode);
    }

    public static bool IsValidCurrencyCode(string currencyCode)
    {
        return ISO._4217.CurrencyCodesResolver.GetCurrenciesByCode(currencyCode).Any();
    }
}

public static class MoneyExtensions
{
    public static Money Sum(this IEnumerable<Money> source)
    {
        return source.Aggregate((x, y) => x + y);
    }

    public static Money Sum<T>(this IEnumerable<T> source, Func<T, Money> selector)
    {
        return source.Select(selector).Aggregate((x, y) => x + y);
    }
}