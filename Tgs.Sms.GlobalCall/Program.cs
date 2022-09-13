using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Tgs.Sms.GlobalCall;
using Tgs.Sms.GlobalCall.Core;
using Tgs.Sms.GlobalCall.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/config/appsettings.json", true);

builder.Services.AddOpenTelemetryTracing(builder =>
    builder
        .AddAspNetCoreInstrumentation()
        .AddSource("TGS.*")
        .AddSource("Tgs.*")
        .AddProcessor(new SimpleActivityExportProcessor(new PerformanceCounterExporter())));

builder.Services
    .AddMessageQueue()
    .AddRabbitMessageQueue(queueConfig =>
    {
        queueConfig
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

        dbContextBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
    });

builder.Services.AddHealthChecks();

builder.Services.AddGlobalCallSms((options, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    config.GetSection("GlobalCallSms").Bind(options);
});

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
});

app.MapHealthChecks("/healthz");

app.Run();
