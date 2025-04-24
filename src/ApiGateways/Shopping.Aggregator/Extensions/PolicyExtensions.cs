using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace Shopping.Aggregator.Extensions
{
    public static class PolicyExtensions
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // in this case will wait for
            // 2 ^ 1 = 2 seconds then
            // 2 ^ 2 = 4 seconds then
            // 2 ^ 3 = 8 seconds an so on


            return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(
                            retryCount: 5,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            onRetry: (exception, retrycount, context) =>
                            {
                                Log.Error($"Retry {retrycount} of {context.PolicyKey} at {context.OperationKey}");
                            });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );
        }
    }
}
