using System.Diagnostics.CodeAnalysis;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Serilog;

namespace SoftCep.Api.Core;

[ExcludeFromCodeCoverage]
public static class SerilogConfiguration
{
    public static void AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithProperty("ApplicationName", "SoftCep")
            .CreateBootstrapLogger();

        Log.Information("Application starting up in {EnvironmentName} mode", builder.Environment.EnvironmentName);


        builder.Logging.ClearProviders();

        if (builder.Environment.IsProduction())
        {
            var url = builder.Configuration.GetValue<string>("Infrastructure:Elasticsearch:Url") ??
                      throw new ArgumentException("Invalid Elasticsearch Url");

            builder.Host.UseSerilog((ctx, _, lc) => lc
                .ReadFrom.Configuration(ctx.Configuration)
                .Enrich.WithProperty("ApplicationName", "SoftCep")
                .WriteTo.Elasticsearch([new Uri(url)], opts =>
                {
                    opts.DataStream = new DataStreamName("logs", "softcep-api", "production");
                    opts.BootstrapMethod = BootstrapMethod.None;
                })
            );
        }
        else
        {
            builder.Host.UseSerilog((ctx, _, lc) => lc
                .ReadFrom.Configuration(ctx.Configuration)
                .Enrich.WithProperty("ApplicationName", "SoftCep"));
        }
    }

    public static void UseSerilogConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
    }
}