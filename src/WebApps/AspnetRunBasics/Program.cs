using AspnetRunBasics.Services;
using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Shopping.Aggregator.Extensions;

namespace AspnetRunBasics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.


            Log.Logger = Serilogger.Configure(builder);

            builder.Services.AddTransient<LoggingDelegatingHandler>();

            builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandler(PolicyExtensions.GetRetryPolicy())
                .AddPolicyHandler(PolicyExtensions.GetCircuitBreakerPolicy());

            builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandler(PolicyExtensions.GetRetryPolicy())
                .AddPolicyHandler(PolicyExtensions.GetCircuitBreakerPolicy());

            builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandler(PolicyExtensions.GetRetryPolicy())
                .AddPolicyHandler(PolicyExtensions.GetCircuitBreakerPolicy());

            builder.Services.AddRazorPages();
            builder.Services.AddHealthChecks()
                            .AddUrlGroup(new Uri(builder.Configuration["ApiSettings:GatewayAddress"]), "Ocelot API Gw", HealthStatus.Degraded);



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();
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