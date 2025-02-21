using System.Diagnostics;
using NHibernate;
using NHibernate.SqlCommand;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Interceptors;

public class SqlDebugOutputInterceptor : EmptyInterceptor
{
    public override SqlString OnPrepareStatement(SqlString sql)
    {
        Debug.Write("NHibernate: ");
        Debug.WriteLine(sql);

        return base.OnPrepareStatement(sql);
    }
}