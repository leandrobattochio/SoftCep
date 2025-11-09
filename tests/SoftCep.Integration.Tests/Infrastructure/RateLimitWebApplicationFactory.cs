using Microsoft.AspNetCore.Hosting;
using SoftCep.Api.Core;

namespace SoftCep.Integration.Tests.Infrastructure;

public class RateLimitWebApplicationFactory : SoftCepWebApplicationFactory
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment(Consts.ProductionEnvironmentName);
    }
}