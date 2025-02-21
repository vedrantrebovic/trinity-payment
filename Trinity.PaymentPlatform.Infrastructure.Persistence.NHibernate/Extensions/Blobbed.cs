using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;
using NpgsqlTypes;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;

[Serializable]
public class Blobbed<T> : IUserType where T : class
{
    public new bool Equals(object x, object y)
    {
        if (x == null && y == null)
            return true;

        if (x == null || y == null)
            return false;

        var xdocX = System.Text.Json.JsonSerializer.Serialize(x);
        var xdocY = System.Text.Json.JsonSerializer.Serialize(y);

        return xdocY == xdocX;
    }

    public int GetHashCode(object x)
    {
        return x == null ? 0 : x.GetHashCode();
    }

    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        if (names.Length != 1)
            throw new InvalidOperationException("Only expecting one column...");

        if (rs[names[0]] is string val && !string.IsNullOrWhiteSpace(val))
            return System.Text.Json.JsonSerializer.Deserialize<T>(val);

        return null;
    }

    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        var parameter = (NpgsqlParameter)cmd.Parameters[index];
        parameter.NpgsqlDbType = NpgsqlDbType.Json;

        if (value == null)
            parameter.Value = DBNull.Value;
        else
            parameter.Value = System.Text.Json.JsonSerializer.Serialize(value);
    }


    public object DeepCopy(object value)
    {
        if (value == null)
            return null;

        var serialized = System.Text.Json.JsonSerializer.Serialize(value);
        return
            System.Text.Json.JsonSerializer.Deserialize<T>(serialized);
    }

    public object Replace(object original, object target, object owner)
    {
        return original;
    }

    public object Assemble(object cached, object owner)
    {
        var str = cached as string;
        return string.IsNullOrWhiteSpace(str)
            ? null
            : System.Text.Json.JsonSerializer.Deserialize<T>(str);
    }

    public object Disassemble(object value)
    {
        return value == null ? null : System.Text.Json.JsonSerializer.Serialize(value);
    }

    public SqlType[] SqlTypes
    {
        //we must write extended SqlType and return it here
        get
        {
            return new SqlType[] { new NpgsqlExtendedSqlType(DbType.Object, NpgsqlDbType.Json) };
        }
    }

    public Type ReturnedType
    {
        get { return typeof(T); }
    }

    public bool IsMutable
    {
        get { return true; }
    }
}