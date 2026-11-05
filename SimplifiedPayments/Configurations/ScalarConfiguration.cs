using Scalar.AspNetCore;

namespace SimplifiedPayments.Configurations;
    
public static class ScalarConfiguration
{
    public static WebApplication UseScalarConfiguration(this WebApplication app)
    {
        app.MapScalarApiReference("/scalar", options =>
        {
            options.WithTitle(ApiInfo.Name);
            options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
        });
        return app;
    }
}
