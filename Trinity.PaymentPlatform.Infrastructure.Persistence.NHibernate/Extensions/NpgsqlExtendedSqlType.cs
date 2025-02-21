using System.Data;
using NHibernate.SqlTypes;
using NpgsqlTypes;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;

public class NpgsqlExtendedSqlType : SqlType
{
    public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType)
        : base(dbType)
    {
        _npgDbType = npgDbType;
    }

    public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType, int length)
        : base(dbType, length)
    {
        _npgDbType = npgDbType;
    }

    public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType, byte precision, byte scale)
        : base(dbType, precision, scale)
    {
        _npgDbType = npgDbType;
    }

    private readonly NpgsqlDbType _npgDbType;
    public NpgsqlDbType NpgDbType => _npgDbType;
}