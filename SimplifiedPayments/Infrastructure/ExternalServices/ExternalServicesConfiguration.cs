using Microsoft.Extensions.Http.Resilience;
using SimplifiedPayments.Application.Abstractions;

namespace SimplifiedPayments.Infrastructure.ExternalServices;

public static class ExternalServicesConfiguration
{
    public static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        services.AddOptions<ExternalServicesOptions>()
            .BindConfiguration(ExternalServicesOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<ITransferAuthorizer, HttpTransferAuthorizer>()
            .AddStandardResilienceHandler(option =>
                Configure(option, attemptSeconds: 2, retries: 1, totalSeconds: 6)
            );

        services.AddHttpClient<INotificationService, HttpNotificationService>()
            .AddStandardResilienceHandler(option => 
                Configure(option, attemptSeconds: 2, retries: 1, totalSeconds: 6)
            );

        return services;
    }

    private static void Configure(
        HttpStandardResilienceOptions option, int attemptSeconds, 
        int retries, int totalSeconds
        )
    {
        option.AttemptTimeout.Timeout = TimeSpan.FromSeconds(attemptSeconds);
        option.Retry.MaxRetryAttempts = retries;
        option.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(totalSeconds);
    }
}
