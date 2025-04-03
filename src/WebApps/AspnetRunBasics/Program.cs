using AspnetRunBasics.Services;
using Common.Logging;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Net.NetworkInformation;
using System.Reflection;

namespace AspnetRunBasics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            
            builder.Services.AddSerilog(Serilogger.Configure(builder));

            builder.Services.AddTransient<LoggingDelegatingHandler>();

            builder.Services.AddHttpClient<ICatalogService, CatalogService>(
                c => c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>();
            builder.Services.AddHttpClient<IBasketService, BasketService>(
                c => c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>();
            builder.Services.AddHttpClient<IOrderService, OrderService>(
                c => c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>();
            builder.Services.AddRazorPages();

           

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

            app.Run();
        }
    }
}