using AcmeExchangeR.API.BackgroundServices;
using AcmeExchangeR.API.Validators;
using AcmeExchangeR.Bus.Services.Abstraction;
using AcmeExchangeR.Bus.Services;
using AcmeExchangeR.Data;
using AcmeExchangeR.Utils.FastForexClient;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ExceptionHandlerMiddleware = AcmeExchangeR.API.Middlewares.ExceptionHandlerMiddleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls("http://*:5010");

builder.Services.AddHttpClient("fastForex", o =>
{
    o.Timeout = TimeSpan.FromMilliseconds(2000);
    o.BaseAddress = new Uri(builder.Configuration.GetValue<string>("FastForexAPI:Url"));
});
builder.Services.AddScoped<IFastForexClient, FastForexClient>();
builder.Services.AddScoped<IRateService, RateService>();
builder.Services.AddScoped<ITradeService, TradeService>();
builder.Services.AddHostedService<RateFetcherBackgroundService>();
builder.Services.AddDbContext<ExchangeRateDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});
builder.Services.AddValidatorsFromAssemblyContaining<TradeValidator>();
builder.Services.AddMemoryCache();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlerMiddleware>();
//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();