using Microsoft.OpenApi;

namespace SimplifiedPayments.Configurations;

public static class OpenAPIConfiguration
{

    public static IServiceCollection AddOpenAPIConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(ApiInfo.Version, new OpenApiInfo
            {
                Title = ApiInfo.Name,
                Version = ApiInfo.Version,
                Description = ApiInfo.Description,
                Contact = new OpenApiContact
                {
                    Name = "William Santana",
                    Url = new Uri("https://williamsantana-portfolio.vercel.app/"),
                    Email = "williamandrade1058@gmail.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/license/mit/")
                }
            });
        });

        return services;
    }
}
