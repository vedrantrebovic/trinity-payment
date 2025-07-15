using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Contracts;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Handlers;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Services;
using Trinity.PaymentPlatform.Model.Contracts;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Extensions;

public static class ServiceExtension
{

    public static IServiceCollection AddAirtelConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AirtelConfig>(configuration.GetSection(AirtelConfig.SectionName));
        return services;
    }

    public static IServiceCollection AddAirtelTokenHandler(this IServiceCollection services)
    {
        services.AddSingleton<IAccessTokenProvider, AirtelAccessTokenProvider>();
        services.AddTransient<AccessTokenHandler>();
        return services;
    }




    public static IServiceCollection AddAirtelHttpClients(this IServiceCollection services)
    {
        var config = services.BuildServiceProvider().GetService<IOptions<AirtelConfig>>()?.Value;
        if (config == null)
            throw new InvalidOperationException("AirtelConfig is not configured properly.");

        services.AddHttpClient("Airtel", client =>
        {
            client.BaseAddress = new Uri(config.ApiUrl);
        }).AddHttpMessageHandler<AccessTokenHandler>();

        services.AddHttpClient("AirtelUnauthorized", client =>
        {
            client.BaseAddress = new Uri(config.ApiUrl);
        });

        return services;


    }

    public static IServiceCollection AddAirtel(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAirtelConfig(configuration);
        services.AddAirtelTokenHandler();
        services.AddAirtelHttpClients();

        services.AddKeyedTransient<ITransactionInitiator, AirtelTransactionInitiator>(AirtelTransactionInitiator.Name);
        services.AddTransient<IAirtelPaymentProvider, AirtelPaymentProvider>();

        return services;
    }


}