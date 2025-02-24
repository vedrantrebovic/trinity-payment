using System.Text.Json;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa
{
    public interface IParameter
    {
        string Key { get; }
        object? RawValue { get; }
    }

    public static class ParameterExtensions
    {
        public static T GetValue<T>(this IEnumerable<IParameter> parameters, string key, T defaultValue = default)
        {
            if (parameters == null)
                return defaultValue;

            var param = parameters.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (param != null && param.RawValue != null)
            {
                if (param.RawValue is T t)
                    return t;

                if (param.RawValue is JsonElement json)
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)json.GetString();
                    }
                    else if (typeof(T) == typeof(decimal))
                    {
                        if (json.ValueKind == JsonValueKind.Number)
                            return (T)(object)json.GetDecimal();
                        else if (json.ValueKind == JsonValueKind.String && decimal.TryParse(json.GetString(), out decimal result))
                            return (T)(object)result;
                    }
                    else
                    {
                        try
                        {
                            return (T)Convert.ChangeType(json.ToString(), typeof(T));
                        }
                        catch
                        {
                            return defaultValue;
                        }
                    }
                }
                else
                {
                    try
                    {
                        return (T)Convert.ChangeType(param.RawValue, typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
            }
            return defaultValue;
        }
    }
}