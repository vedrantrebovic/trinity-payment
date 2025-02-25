using Npgsql;
using Quartz;
using Serilog;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;
using Trinity.PaymentPlatform.Processors.Outbox;
using Trinity.PaymentPlatform.Processors.Outbox.Jobs;
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

builder.Services.AddHttpClient();

builder.Services.AddNHibernate();
builder.Services.AddPaymentRepositories();

builder.Services.AddQuartz(q =>
{
    q.AddJobAndTrigger<CreatePaymentOutboxJob>(builder.Configuration);
    q.AddJobAndTrigger<SendOutboxJob>(builder.Configuration);
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = $"Payment platform outbox processor";
});

var host = builder.Build();
host.Run();
