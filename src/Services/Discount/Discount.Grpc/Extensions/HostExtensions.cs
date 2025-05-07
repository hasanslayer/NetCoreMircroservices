using Microsoft.Extensions.Configuration;
using Npgsql;
using Polly;
using Serilog;

namespace Discount.Grpc.Extensions
{
    public static class HostExtensions
    {
        public static WebApplication MigrateDatabase<TContext>(this WebApplication host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();

                try
                {

                    Log.Information("Migrating postgresql database associated with context {DbContextName}", typeof(TContext).Name);

                    // apply polly retry policy for migration
                    var retry = Policy.Handle<NpgsqlException>()
                                                .WaitAndRetry(
                                                    retryCount: 5,
                                                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                                    onRetry: (exception, retrycount, context) =>
                                                    {
                                                        Log.Error($"Retry {retrycount} of {context.PolicyKey} at {context.OperationKey}");
                                                    });


                    retry.Execute(() => ExecuteMigration(configuration));



                    Log.Information("Migrating postgresql database.");

                }
                catch (NpgsqlException ex)
                {
                    Log.Error(ex, "An error occurred while migrating the postresql database");
                }
            }

            return host;
        }

        private static void ExecuteMigration(IConfiguration configuration)
        {
            using var connetion = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            connetion.Open();
            using var command = new NpgsqlCommand
            {
                Connection = connetion
            };
            command.CommandText = "DROP TABLE IF EXISTS Coupon";
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                                ProductName VARCHAR(24) NOT NULL,
                                                                Description TEXT,
                                                                Amount INT)";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
            command.ExecuteNonQuery();
        }
    }
}
