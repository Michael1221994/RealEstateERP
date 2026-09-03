using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastracture.Base.API.Extentions
{
    public static class HangfireExtensions
    {
        public static IServiceCollection AddSharedHangfire(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(
                        configuration.GetConnectionString("DefaultConnection"))));

            var serverEnabled = configuration.GetValue("Hangfire:ServerEnabled", true);
            if (serverEnabled)
            {
                services.AddHangfireServer(options =>
                {
                    options.WorkerCount = 5; // adjust based on your load
                });
            }

            return services;
        }
    }
}
