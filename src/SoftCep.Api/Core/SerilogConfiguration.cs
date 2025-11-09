using Serilog;

namespace SoftCep.Api.Core;

public static class SerilogConfiguration
{
    public static void AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Application starting up in {EnvironmentName} mode", builder.Environment.EnvironmentName);

        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((ctx, serviceProvider, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.WithProperty("ApplicationName", "SoftCep"));
    }

    public static void UseSerilogConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
    }
}