using System.Reflection;
using Common.Logging;
using Discount.Grpc.Extensions;
using Discount.Grpc.Mapper;
using Discount.Grpc.Repositories;
using Discount.Grpc.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Discount.Grpc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.

            Log.Logger = Serilogger.Configure(builder);

            builder.Services.AddGrpc();
            builder.Services.AddAutoMapper(typeof(DiscountProfile));
            builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
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

            var app = builder.Build();
            app.MigrateDatabase<Program>();


            // Configure the HTTP request pipeline.
            app.MapGrpcService<DiscountService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}