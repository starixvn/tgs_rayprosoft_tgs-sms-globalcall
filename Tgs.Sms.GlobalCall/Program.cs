using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Sms.GlobalCall.Core;
using Tgs.Sms.GlobalCall.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp!;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddOtlpExporter())
    .WithMetrics(meterBuiler => meterBuiler
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .Services
    .Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue
            || !httpContext.Request.Path.StartsWithSegments("/swagger")
            && !httpContext.Request.Path.StartsWithSegments("/healthz")
            && !httpContext.Request.Path.StartsWithSegments("/profiler/")
            && !httpContext.Request.Path.StartsWithSegments("/hangfire")
            && !httpContext.Request.Path.StartsWithSegments("/metrics")
            && !new[] { "html", "ico", "png", "jpg" }.Any(
                ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

builder.Services
    .AddMessageQueue()
    .AddRabbitMessageQueue(queueConfig =>
    {
        _ = queueConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                config.GetSection("Sms").Bind(options);

                options.OnSetupQueueAndExchange = channel =>
                {
                    _ = channel.QueueDeclare(
                        "tgs.sms.globalcall",
                        true,
                        false,
                        false,
                        null);

                    channel.QueueBind(
                        "tgs.sms.globalcall",
                        "amq.direct",
                        "tgs.sms.globalcall",
                        null);
                };
            })
            .AddHandler<GlobalCallReceiveHandler>("tgs.sms.globalcall", false);
    })
    .AddRabbitGlobPatternExchange("*", "amq.direct");

builder.Services
    .AddSingleton<GlobalCallSerializer>()
    .AddGlobalCallRepositories(
    (sp, dbContextBuilder) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        _ = dbContextBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
    });

builder.Services.AddHealthChecks();

builder.Services.AddGlobalCallSms((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    config.GetSection("GlobalCallSms").Bind(options);
});

var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "Hello Sms GlobalCall");

app.MapHealthChecks("/healthz");

app.Run();
