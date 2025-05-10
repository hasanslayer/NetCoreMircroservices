using Common.Logging;
using EventBus.Messages.Common;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering.API.EventBussConsumer;
using Ordering.API.Extensions;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Serilog;
using System.Reflection;

namespace Ordering.API
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
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
            builder.Services.AddScoped<BasketCheckoutConsumer>();

            // MassTransit-RabbitMQ Configuration
            builder.Services.AddMassTransit(config =>
            {
                // now here we would consume rabbit mq or subscribe to it
                config.AddConsumer<BasketCheckoutConsumer>();
                config.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(builder.Configuration["EventBusSettings:HostAddress"]); // username and password is : guest
                    configurator.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
                     {
                         c.ConfigureConsumer<BasketCheckoutConsumer>(context);
                     });
                });
            });
            builder.Services.AddMassTransitHostedService();
            builder.Services.AddHealthChecks()
                            .AddSqlServer(builder.Configuration.GetConnectionString("OrderingConnectionString"));

            builder.Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown Service"))
                    .AddZipkinExporter(options =>
                    {
                        //options.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); // Default Zipkin endpoint
                        options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans"); // after add zipkin into docker compose
                    });
            });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.MigrateDatabase<OrderContext>((context,services) =>
            {
                var logger = services.GetService<ILogger<OrderContextSeed>>();
                OrderContextSeed.SeedAsync(context,logger).Wait();
            });

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