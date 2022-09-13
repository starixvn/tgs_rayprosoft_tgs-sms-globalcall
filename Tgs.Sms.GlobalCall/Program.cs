using Microsoft.EntityFrameworkCore;
using Tgs.Sms.GlobalCall.Core;
using Tgs.Sms.GlobalCall.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/config/appsettings.json", true);

builder.Services.AddRabbitMessageQueue(queueConfig =>
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
    });

builder.Services
    .AddSingleton<GlobalCallSerializer>()
    .AddGlobalCallRepositories(
    (sp, dbContextBuilder) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        dbContextBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
    });

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

app.Run();
