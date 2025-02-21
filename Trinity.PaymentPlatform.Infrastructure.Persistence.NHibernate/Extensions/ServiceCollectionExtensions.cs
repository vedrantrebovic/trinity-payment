using Microsoft.Extensions.DependencyInjection;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Repositories;
using Trinity.PaymentPlatform.Model.PaymentProviderAggregate;
using Trinity.PaymentPlatform.Model.PaymentTransactionAggregate;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNHibernate(this IServiceCollection services)
    {
        services.AddSingleton<INHibernateHelper, NHibernateHelper>();
        services.AddScoped<IUnitOfWork, NHUnitOfWork>();
        return services;
    }

    public static IServiceCollection AddPaymentRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IPaymentProviderRepository, PaymentProviderRepository>();
        return services;
    }
}