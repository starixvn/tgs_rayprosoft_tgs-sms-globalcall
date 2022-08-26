using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tgs.Sms.GlobalCall.Core;
using Tgs.Sms.GlobalCall.MessageQueue;

namespace Tgs.Sms.GlobalCall
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddRabbitMessageQueue(queueConfig =>
                {
                    queueConfig
                        .ConfigQueueOptions((options, sp) =>
                        {
                            var config = sp.GetRequiredService<IConfiguration>();

                            config.GetSection("Sms").Bind(options);

                            options.OnSetupQueueAndExchange = channel =>
                            {
                                channel.QueueDeclare(
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

            services
                .AddSingleton<GlobalCallSerializer>()
                .AddGlobalCallRepositories(
                (sp, dbContextBuilder) =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();

                    dbContextBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
                });

            services.AddGlobalCallSms((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                config.GetSection("GlobalCallSms").Bind(options);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
