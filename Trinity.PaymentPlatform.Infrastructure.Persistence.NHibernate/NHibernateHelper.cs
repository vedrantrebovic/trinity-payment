using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Interceptors;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Mappings;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Utils;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate;

public class NHibernateHelper(IConfiguration configuration) : INHibernateHelper
    {
        private readonly string _connectionString = configuration["ConnectionStrings:default_payment"];
        private readonly object _lockObject = new object();
        private ISessionFactory? _sessionFactory;
        private short _batchSize = 20;

    private ISessionFactory? SessionFactory
        {
            get
            {
                lock (_lockObject)
                {
                    if (_sessionFactory == null)
                    {
                        CreateSessionFactory();
                    }

                    return _sessionFactory;
                }
            }
        }

        public ISession OpenSession()
        {
            return SessionFactory.OpenSession();
#if DEBUG
        return SessionFactory.WithOptions().Interceptor(new SqlDebugOutputInterceptor()).OpenSession();
#else
            return SessionFactory.OpenSession();
#endif
    }

    public IStatelessSession OpenStatelessSession()
        {
            return SessionFactory.OpenStatelessSession();
        }

        private void CreateSessionFactory()
        {

            if (!string.IsNullOrEmpty(configuration["AdoNet:BatchSize"]))
                _batchSize = Convert.ToInt16(configuration["AdoNet:BatchSize"]);

            var fluentConfiguration = Fluently.Configure()
                .Database(
                    PostgreSQLConfiguration.Standard.Dialect<PostgreSQL83Dialect>()
                        .ConnectionString(_connectionString)
#if DEBUG
                        .ShowSql().AdoNetBatchSize(20)   
#endif
                )
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<PaymentProviderMap>())
                //.ExposeConfiguration(cfg => new SchemaExport(cfg).Execute(true, true, false))
                .ExposeConfiguration(cfg =>
                {
#if DEBUG
                    //new SchemaExport(cfg).Execute(true,true, true);
                    new SchemaUpdate(cfg).Execute(false, true);
#endif
                    cfg.DataBaseIntegration(x =>
                    {
                        x.BatchSize = _batchSize;
                    });

                    //cfg.SetProperty(NHibernate.Cfg.Environment.FormatSql, Boolean.FalseString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.GenerateStatistics, Boolean.FalseString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.PrepareSql, Boolean.TrueString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.PropertyUseReflectionOptimizer, Boolean.TrueString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.QueryStartupChecking, Boolean.FalseString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.UseProxyValidator, Boolean.FalseString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.UseSecondLevelCache, Boolean.FalseString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.UseSqlComments, Boolean.FalseString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.UseQueryCache, Boolean.FalseString);
                    //cfg.SetProperty(NHibernate.Cfg.Environment.WrapResultSets, Boolean.TrueString);

                    // cfg.EventListeners.PostInsertEventListeners = new IPostInsertEventListener[]
                    //     { new NHEventListener(_mediator) };
                    // cfg.EventListeners.PostDeleteEventListeners = new IPostDeleteEventListener[]
                    //     { new NHEventListener(_mediator) };
                    // cfg.EventListeners.PostUpdateEventListeners = new IPostUpdateEventListener[]
                    //     { new NHEventListener(_mediator) };
                    // cfg.EventListeners.PostCollectionUpdateEventListeners = new IPostCollectionUpdateEventListener[]
                    //     { new NHEventListener(_mediator) };
                    cfg.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[]
                        { new AuditEventListener() };
                    cfg.EventListeners.PreInsertEventListeners = new IPreInsertEventListener[]
                        { new AuditEventListener() };
                });

            _sessionFactory = fluentConfiguration.BuildSessionFactory();
            
        }
    }