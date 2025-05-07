using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;

namespace Ordering.API.Extensions
{
    public static class HostExtensions
    {
        public static WebApplication MigrateDatabase<TContext>(this WebApplication webApplication,
            Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
           
            using (var scope = webApplication.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                    // apply polly retry policy for migration
                    var retry = Policy.Handle<SqlException>()
                                                .WaitAndRetry(
                                                    retryCount: 5,
                                                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                                    onRetry: (exception, retrycount, context) =>
                                                    {
                                                        Log.Error($"Retry {retrycount} of {context.PolicyKey} at {context.OperationKey}");
                                                    });

                    // if the sql server container is not created on run docker compose 
                    // this migration can't fail for network related exception. the retry options for DbContext only
                    // apply the transient exceptions
                    // Note that this is NOT applied when running some orchestrators (let the orchestrator to recreate the failing service)

                    retry.Execute(() => InvokeSeeder(seeder, context, services));



                    Log.Information("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
                }
                catch (SqlException ex)
                {
                    Log.Error(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                }
            }

            return webApplication;
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext? context, IServiceProvider services) where TContext : DbContext
        {
            context.Database.Migrate();
            seeder(context, services);
        }
    }
}
