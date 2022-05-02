using System.Net;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace SkinApi.Gui.Clients;

public class Policies
{
    internal static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
    {
        return CreateRetryPolicy(BuildDefaultHttpErrorHandling());
    }

    internal static IAsyncPolicy<HttpResponseMessage> GetDefaultCircuitBreakerPolicy()
    {
        return BuildDefaultHttpErrorHandling()
            .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, TimeSpan.FromSeconds(30));
    }

    internal static PolicyBuilder<HttpResponseMessage> BuildDefaultHttpErrorHandling()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests);
    }

    internal static AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy(
        PolicyBuilder<HttpResponseMessage> policy)
    {
        return policy.WaitAndRetryAsync(retryCount: 3,
            CalculateSecondsToWaitForRetry);
    }

    internal static TimeSpan CalculateSecondsToWaitForRetry(int retryAttempt)
    {
        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
    }
}