using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace Trinity.PaymentProvider.API.Shared.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddOpenIddictServer(this IServiceCollection services, string issuer, List<SecurityKey> signingKeys, 
        params string[] audiences)
    {
        services.AddOpenIddict(oidcBuilder =>
        {
            oidcBuilder.AddValidation(builder =>
            {
                builder.SetIssuer(issuer);
                builder.UseSystemNetHttp();
                builder.AddAudiences(audiences);
                builder.Configure(options => { });
                var conf = new OpenIddictConfiguration() { Issuer = new Uri(issuer)};
                //conf.SigningKeys.AddRange(signingKeys);
                builder.SetConfiguration(conf);
                
                //todo: add encryption and signing certificates

                builder.UseAspNetCore();
            });
        });
        return services;
    }

    public static IServiceCollection AddOpenIddictValidation(this IServiceCollection services, string issuer, IList<string> audiences, 
        X509Certificate2 signingCert, X509Certificate2 encryptionCert )
    {
        services.AddOpenIddict(oidcBuilder =>
        {
            oidcBuilder.AddValidation(validationBuilder =>
            {
                validationBuilder.SetIssuer(issuer);
                validationBuilder.UseSystemNetHttp();
                validationBuilder.AddAudiences(audiences.ToArray());
                validationBuilder.Configure(options =>
                {
          
                });
                validationBuilder.SetConfiguration(new OpenIddictConfiguration()
                {
                    Issuer = new Uri(issuer),
                    SigningKeys = { new X509SecurityKey(signingCert) }
                });
                validationBuilder.AddEncryptionCertificate(encryptionCert);
                validationBuilder.UseAspNetCore();
            });
        });
        
        return services;
    }
}