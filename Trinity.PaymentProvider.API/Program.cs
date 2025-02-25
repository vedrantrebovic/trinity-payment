using NHibernate.Transaction;
using Npgsql;
using Serilog;
using Trinity.PaymentPlatform.Application.Commands;
using Trinity.PaymentPlatform.Application.Factory;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Extensions;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;
using Trinity.PaymentPlatform.Model.Contracts;
using Trinity.PaymentPlatform.Mpesa.Application.Commands;
using Trinity.PaymentPlatform.Shared;
using Trinity.PaymentProvider.API.ModelBinders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.Services.AddMvc(p =>
//{
//    p.ModelBinderProviders.Insert(0, new PayInModelBinderProvider());
//    p.ModelBinderProviders.Insert(1, new PayoutModelBinderProvider());
//});

builder.Host.UseSerilog((_, configuration) =>
{
    configuration.ConfigureBaseLogging("payment_platform_api", builder.Configuration, builder.Environment.ContentRootPath);
});

builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("default"), sourceBuilder =>
{
    sourceBuilder.UseJsonNet();
});

builder.Services.AddMemoryCache();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(CreatePayinRequestCommand).Assembly, typeof(ProcessB2CResultCommand).Assembly));
//builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePayinRequestCommand>());
builder.Services.AddNHibernate();
builder.Services.AddPaymentRepositories();
builder.Services.AddMpesa(builder.Configuration);
builder.Services.AddTransient<ITransactionInitiatorFactory, TransactionInitiatorFactory>();
builder.Services.AddTransient<IPayInTransactionInitiationParamsConverter, PayInTransactionInitiationParamsConverter>();
builder.Services.AddTransient<IPayoutTransactionInitiationParamsConverter, PayoutTransactionInitiationParamsConverter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
