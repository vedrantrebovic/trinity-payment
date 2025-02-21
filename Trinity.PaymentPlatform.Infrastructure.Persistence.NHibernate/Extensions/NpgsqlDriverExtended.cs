using System.Data.Common;
using NHibernate;
using NHibernate.Driver;
using NHibernate.SqlTypes;
using Npgsql;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;

public class NpgsqlDriverExtended : NpgsqlDriver
{
    protected override void InitializeParameter(DbParameter dbParam, string name, SqlType sqlType)
    {
        if (sqlType is NpgsqlExtendedSqlType type && dbParam is NpgsqlParameter param)
            InitializeParameter(param, name, type);
        else
            base.InitializeParameter(dbParam, name, sqlType);
    }

    protected virtual void InitializeParameter(NpgsqlParameter dbParam, string name, NpgsqlExtendedSqlType sqlType)
    {
        if (sqlType == null)
            throw new QueryException($"No type assigned to parameter '{name}'");

        dbParam.ParameterName = FormatNameForParameter(name);
        dbParam.DbType = sqlType.DbType;
        dbParam.NpgsqlDbType = sqlType.NpgDbType;
    }
}