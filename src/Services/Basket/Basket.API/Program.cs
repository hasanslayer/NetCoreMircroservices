using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Common.Logging;
using Discount.Grpc.Protos;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System.Reflection;

namespace Basket.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            Log.Logger = Serilogger.Configure(builder);

            builder.Services.AddAuthorization();
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IBasketRepository, BasketRepository>();
            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
            builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(
                o => o.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]));

            builder.Services.AddScoped<DiscountGrpcService>();

            // MassTransit-RabbitMQ Configuration
            builder.Services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(builder.Configuration["EventBusSettings:HostAddress"]); // username and password is : guest
                    configurator.UseHealthCheck(context); // for health check of rabbitMQ
                });
            });
            builder.Services.AddMassTransitHostedService();
            builder.Services.AddHealthChecks()
                            .AddRedis(builder.Configuration["CacheSettings:ConnectionString"], "Redist Health", HealthStatus.Degraded);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/hc", new HealthCheckOptions
            {
                // to return as a json format
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.Run();
        }
    }
}