using Common.Logging;
using Discount.API.Extensions;
using Discount.API.Repositories;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace Discount.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            Log.Logger = Serilogger.Configure(builder);

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
            builder.Services.AddHealthChecks()
                            .AddNpgSql(builder.Configuration["DatabaseSettings:ConnectionString"]);

            var app = builder.Build();
            app.MigrateDatabase<Program>();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();
            app.UseHealthChecks("/hc", new HealthCheckOptions
            {
                // to return as a json format
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.Run();
        }
    }
}