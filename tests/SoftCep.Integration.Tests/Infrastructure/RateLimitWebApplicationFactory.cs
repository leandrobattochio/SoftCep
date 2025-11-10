using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using SoftCep.Api.Core;

namespace SoftCep.Integration.Tests.Infrastructure;

public class RateLimitWebApplicationFactory : WebApplicationFactory<SoftCep.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment(Consts.ProductionEnvironmentName);
    }
}