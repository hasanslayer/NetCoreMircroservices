using Common.Logging;
using Discount.Grpc.Extensions;
using Discount.Grpc.Mapper;
using Discount.Grpc.Repositories;
using Discount.Grpc.Services;
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

            builder.Services.AddSerilog(Serilogger.Configure(builder));

            builder.Services.AddGrpc();
            builder.Services.AddAutoMapper(typeof(DiscountProfile));
            builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();

            var app = builder.Build();
            app.MigrateDatabase<Program>();


            // Configure the HTTP request pipeline.
            app.MapGrpcService<DiscountService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}