using Microsoft.EntityFrameworkCore;

namespace SimplifiedPayments.Configurations;

public static class CorsConfiguration
{
    private static string PolicyName = "DefaultCors";

    private static string[] GetAllowedOrigins(IConfiguration configuration)
    {
        var origins = configuration["AllowedOrigins"];

        if (string.IsNullOrWhiteSpace(origins)) return [];
        
        return origins.Split(',', 
            StringSplitOptions.RemoveEmptyEntries | 
            StringSplitOptions.TrimEntries
        );
    }
       
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services, IConfiguration configuration
        )
    {
        string[] allowedOrigins = GetAllowedOrigins(configuration);

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

    public static IApplicationBuilder UseCorsConfiguration(
        this IApplicationBuilder app, IConfiguration configuration
    )
    {
        var origins = GetAllowedOrigins(configuration);

        app.Use(async (context, next) =>
        {
            var origin = context.Request.Headers.Origin.ToString();
            var sameOrigin = $"{context.Request.Scheme}://{context.Request.Host}";

            if (!string.IsNullOrEmpty(origin)
                && !origin.Equals(sameOrigin, StringComparison.OrdinalIgnoreCase)
                && !origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CORS origin not allowed.");
                return;
            }
            await next();
        });
        app.UseCors(PolicyName);
        return app;
    }

}
