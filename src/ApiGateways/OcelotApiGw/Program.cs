using System.Reflection;
using Common.Logging;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true);

//builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();

builder.Services.AddOcelot().AddCacheManager(x => x.WithDictionaryHandle());

Log.Logger = Serilogger.Configure(builder);

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
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

await app.UseOcelot();

app.MapGet("/", () => "Hello World!");




app.Run();
