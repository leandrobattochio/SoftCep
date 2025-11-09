using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;
using Scalar.AspNetCore;
using SoftCep.Api.Application;
using SoftCep.Api.Core;
using SoftCep.Api.Infrastructure.ViaCep;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.EnvironmentName != Consts.TestEnvironmentName)
    builder.AddSerilogConfiguration();

builder.Services.AddControllers();
builder.Services.AddHybridCache();
builder.Services.AddHealthChecks();
builder.Services.AddOptions<ViaCepClientOptions>().BindConfiguration("Infrastructure:ViaCep");
builder.Services.AddTransient<GetCepQueryHandler>();
builder.Services.AddTransient<GetCepFromAddressQueryHandler>();
builder.Services.AddRefitClient<IViaCepClient>().ConfigureHttpClient((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ViaCepClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
    .WaitAndRetryAsync(Consts.HttpRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));


if (builder.Environment.EnvironmentName != Consts.TestEnvironmentName)
    builder.Services.AddRateLimitConfiguration();

builder.Services.AddOpenApi();

var app = builder.Build();
if (builder.Environment.EnvironmentName != Consts.TestEnvironmentName)
    app.UseSerilogConfiguration();

app.UseHealthChecks("/healthz");

// Rate limiter middleware somente fora de Testing
if (app.Environment.EnvironmentName != Consts.TestEnvironmentName)
    app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

namespace SoftCep.Api
{
    public partial class Program
    {
    }
}