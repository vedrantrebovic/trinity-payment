using NHibernate;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate;

public interface INHibernateHelper
{
    ISession OpenSession();
    IStatelessSession OpenStatelessSession();
}