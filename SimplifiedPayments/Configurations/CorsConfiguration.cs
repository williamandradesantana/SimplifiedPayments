namespace SimplifiedPayments.Configurations;

public static class CorsConfiguration
{
    private static string PolicyName = "DefaultCors";

    private static string[] GetAllowedOrigns(IConfiguration configuration)
    {
        return configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    }
    
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services, IConfiguration configuration
        )
    {
        string[] allowedOrigins = GetAllowedOrigns(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
              policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
            );
        });
        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
       => app.UseCors(PolicyName);
    
}
