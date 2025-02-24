using System.Text.RegularExpressions;
using Trinity.PaymentPlatform.Application.Shared.Dto;

namespace Trinity.PaymentPlatform.Application.Shared.Extensions;

public static class EnumExtension
{
    public static List<EnumListDto> GetMembersList<T>() where T:System.Enum
    {
        List<EnumListDto> result = new();
        foreach (var enumMember in Enum.GetValues(typeof(T)))
        {
            string key = Regex.Replace(enumMember.ToString(), @"(?<!_)([A-Z])", "_$1");
            result.Add(new((int)enumMember, $"lbl:{key.ToLower().TrimStart('_')}"));
        }

        return result;
    }
}