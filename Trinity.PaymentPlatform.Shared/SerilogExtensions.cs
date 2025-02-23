using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Trinity.PaymentPlatform.Shared;

public static class SerilogExtensions
{
    public static LoggerConfiguration ConfigureBaseLogging(
        this LoggerConfiguration loggerConfiguration,
        string appName, IConfiguration configuration, string? basePath = null)
    {
#if DEBUG
        string logPath = AppDomain.CurrentDomain.BaseDirectory;
#else
    string logPath = basePath ?? AppDomain.CurrentDomain.BaseDirectory;
#endif
        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Code))
            .WriteTo.Map(evt => evt.Level, ((level, cfg) =>
            {
                cfg.File($"{logPath}/logs/{level.ToString().ToLower()}/{level.ToString().ToLower()}.txt",
                        rollingInterval: RollingInterval.Day)
                    .MinimumLevel.Information().Enrich.FromLogContext();
            }))
            .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(p => p.MessageTemplate.Text.StartsWith("[REQUEST]"))
                .WriteTo.File($"{logPath}/logs/requests/req.txt", rollingInterval: RollingInterval.Day));

        var seqSection = configuration.GetSection("Seq");
        if(seqSection.Exists())
        {
            loggerConfiguration.WriteTo.Seq(configuration["Seq:Url"], LogEventLevel.Warning,
                apiKey: configuration["Seq:APIKey"]);
        }

        loggerConfiguration.Enrich.FromLogContext()
        // Build information as custom properties
        .Enrich.WithProperty("ApplicationName", appName)
        .Enrich.WithProperty("UTC", DateTime.UtcNow);

        return loggerConfiguration;
    }

    public static LoggerConfiguration AddApplicationInsightsLogging(this LoggerConfiguration loggerConfiguration, IServiceProvider services, IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration["ApplicationInsights:InstrumentationKey"]))
        {
            loggerConfiguration.WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces);
        }

        return loggerConfiguration;
    }
}