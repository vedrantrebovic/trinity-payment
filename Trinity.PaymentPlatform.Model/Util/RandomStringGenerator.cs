using System.Security.Cryptography;

namespace Trinity.PaymentPlatform.Model.Util;

public class RandomStringGenerator
{
    private const string _lowerCaseAlphanumeric = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const string _upperCaseAlphanumeric = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string _numeric = "0123456789";
    const string _alphanumeric = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string _chars = "abcdefghijklmnopqrstuvwxyz0123456789!\"#$%&'()*@[\\]^_`{|}~ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string _charsNoQuotes = "abcdefghijklmnopqrstuvwxyz0123456789!#$%&()*@[\\]^_{|}~ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string _urlSafeChars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-_~";

    public static string GenerateLowerCaseAlphanumeric(int length)=> Generate(length, _lowerCaseAlphanumeric.AsSpan());
    public static string GenerateUpperCaseAlphanumeric(int length)=>Generate(length, _upperCaseAlphanumeric.AsSpan());
    public static string GenerateNumeric(int length)=>Generate(length, _numeric.AsSpan());
    public static string GenerateAlphanumeric(int length)=> Generate(length, _alphanumeric.AsSpan());
    public static string Generate(int length)=> Generate(length, _chars.AsSpan());
    public static string GenerateUrlSafe(int length)=> Generate(length, _urlSafeChars);
    private static string Generate(int length, ReadOnlySpan<char> choices)=> RandomNumberGenerator.GetString(choices, length);
}