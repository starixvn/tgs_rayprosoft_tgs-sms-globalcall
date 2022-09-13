using Microsoft.Extensions.Options;
using Tgs.Sms.GlobalCall;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGlobalCallSms(
    this IServiceCollection services,
    Action<GlobalCallSmsOptions, IServiceProvider> configure)
    => services.AddOptions<GlobalCallSmsOptions>()
        .Configure(configure)
        .Services
        .AddHttpClient<GlobalCallClient>()
        .ConfigureHttpClient((sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<GlobalCallSmsOptions>>().Value;

            httpClient.BaseAddress = options.SmsApiUrl;
        })
        .Services;
}
