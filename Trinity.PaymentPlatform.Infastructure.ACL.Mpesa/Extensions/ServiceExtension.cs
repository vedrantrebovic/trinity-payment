using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Handlers;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Services;
using Trinity.PaymentPlatform.Model.Contracts;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddMpesaConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MpesaConfig>(configuration.GetSection(MpesaConfig.SectionName));
        return services;
    }

    public static IServiceCollection AddMpesaTokenHandler(this IServiceCollection services)
    {
        services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
        services.AddTransient<AccessTokenHandler>();
        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        MpesaConfig config = services.BuildServiceProvider().GetService<IOptions<MpesaConfig>>().Value;
        services.AddHttpClient("Mpesa", client => client.BaseAddress = new Uri(config.ApiUrl)).AddHttpMessageHandler<AccessTokenHandler>();
        services.AddHttpClient("MpesaUnauthorized", client => client.BaseAddress = new Uri(config.ApiUrl));
        return services;
    }

    public static IServiceCollection AddMpesa(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMpesaConfig(configuration);
        services.AddMpesaTokenHandler();
        services.AddHttpClients();
        services.AddKeyedTransient<ITransactionInitiator, MpesaTransactionInitiator>(MpesaTransactionInitiator.Name);
        services.AddTransient<IMpesaPaymentProvider, MpesaPaymentProvider>();

        return services;
    }
}