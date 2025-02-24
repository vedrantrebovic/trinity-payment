using Npgsql;
using Serilog;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Extensions;
using Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Extensions;
using Trinity.PaymentPlatform.Mpesa.Application.Commands;
using Trinity.PaymentPlatform.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Host.UseSerilog((_, configuration) =>
{
    configuration.ConfigureBaseLogging("payment_platform_api", builder.Configuration, builder.Environment.ContentRootPath);
});

builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("default"), sourceBuilder =>
{
    sourceBuilder.UseJsonNet();
});

builder.Services.AddMemoryCache();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePayinRequestCommand>());
builder.Services.AddNHibernate();
builder.Services.AddPaymentRepositories();
builder.Services.AddMpesa(builder.Configuration);

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
