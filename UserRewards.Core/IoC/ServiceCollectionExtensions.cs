using UserRewards.Core.Interfaces;
using UserRewards.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using UserRewards.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace UserRewards.Core.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureUserRewardsCore(this IServiceCollection services)
        {
            services.AddDbContext<RewardsDbContext>(options => options.UseInMemoryDatabase("Rewards"));
            services.AddScoped<IRewardsService, RewardsService>();

            return services;
        }
    }
}
