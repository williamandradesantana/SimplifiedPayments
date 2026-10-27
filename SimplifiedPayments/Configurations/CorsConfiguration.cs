using Microsoft.EntityFrameworkCore;

namespace SimplifiedPayments.Configurations;

public static class CorsConfiguration
{
    private static string PolicyName = "DefaultCors";

    private static string[] GetAllowedOrigins(IConfiguration configuration)    
        => configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
       
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
            var origin = context.Request.Headers["Origin"].ToString();
            if (
                !string.IsNullOrEmpty(origin) && !origins.Contains(origin, StringComparer.OrdinalIgnoreCase)
               )
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
