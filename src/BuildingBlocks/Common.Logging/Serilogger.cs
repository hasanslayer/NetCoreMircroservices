using Microsoft.AspNetCore.Builder;
using Serilog.Sinks.Elasticsearch;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;

namespace Common.Logging
{
    public static class Serilogger
    {
        public static Logger Configure(WebApplicationBuilder builder)
        {
            

            var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(
                 new ElasticsearchSinkOptions(new Uri(builder.Configuration["ElasticConfiguration:Uri"]))
                 {
                     IndexFormat = $"applogs-{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{builder.Environment.EnvironmentName?.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}",
                     AutoRegisterTemplate = true,
                     NumberOfShards = 2,
                     NumberOfReplicas = 1,
                     AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                     OverwriteTemplate = true,
                 })
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();


            return loggerConfig;
        }
    }
}
