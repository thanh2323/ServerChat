using System;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuMind.Infrastructure.Extention
{
    public static class HangfireConfiguration
    {
        public static IServiceCollection AddHangfireConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHangfire(config =>
            {
                config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
            configuration.GetConnectionString("DefaultConnection"),
            new PostgreSqlStorageOptions
            {
                SchemaName = "hangfire",
                InvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(5),
                PrepareSchemaIfNecessary = true
            }
        );
            });
            return services;
        }

        public static IServiceCollection AddHangfireServerWithConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection("HangfireSettings");

            var serverName = section.GetValue<string>("ServerName") ?? "DocuMind-Worker";
            var workerCount = section.GetValue<int?>("WorkerCount") ?? 5;
            var queues = section.GetSection("Queues").Get<string[]>()
                         ?? new[] { "default", "processing" };

            services.AddHangfireServer(options =>
            {
                options.ServerName = serverName;
                options.WorkerCount = workerCount;
                options.Queues = queues;
            });

            return services;
        }
    }
}
