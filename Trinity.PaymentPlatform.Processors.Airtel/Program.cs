using Npgsql;
using Quartz;
using Serilog;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Extensions;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;
using Trinity.PaymentPlatform.Processors.Airtel;
using Trinity.PaymentPlatform.Processors.Airtel.Jobs;
using Trinity.PaymentPlatform.Shared;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

builder.Services.AddSerilog(cfg =>
{
    cfg.ConfigureBaseLogging("payment_platform_processor", builder.Configuration);
});

builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("default_payment"), sourceBuilder =>
{
    sourceBuilder.UseJsonNet();
});

builder.Services.Configure<AirtelConfig>(builder.Configuration.GetSection(AirtelConfig.SectionName));
builder.Services.AddTransient<HttpLoggingHandler>();
builder.Services.AddHttpClient("Airtel")
    .AddHttpMessageHandler<HttpLoggingHandler>();

builder.Services.AddMemoryCache();
builder.Services.AddNHibernate();
builder.Services.AddPaymentRepositories();
builder.Services.AddAirtel(builder.Configuration);

builder.Services.AddQuartz(q =>
{
    q.AddJobAndTrigger<PayInCheckJob>(builder.Configuration);
    q.AddJobAndTrigger<PayOutCheckJob>(builder.Configuration);
    q.AddJobAndTrigger<ProcessPayInTransactionJob>(builder.Configuration);
    q.AddJobAndTrigger<ProcessPayoutTransactionJob>(builder.Configuration);
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


builder.Services.AddWindowsService(options =>
{
    options.ServiceName = $"Payment platform Airtel transaction processor";
});


var host = builder.Build();
host.Run();
