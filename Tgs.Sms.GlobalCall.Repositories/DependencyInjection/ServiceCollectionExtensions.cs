using System;
using Microsoft.EntityFrameworkCore;
using Tgs.Sms.GlobalCall;
using Tgs.Sms.GlobalCall.Repositories;
using Tgs.Sms.GlobalCall.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGlobalCallRepositories(
            this IServiceCollection services,
            Action<IServiceProvider, DbContextOptionsBuilder> dbContextConfigure)
            => services
                .AddDbContext<TgsContext>(dbContextConfigure)
                .AddTransient<ITgsRepository, TgsRepository>();
    }
}