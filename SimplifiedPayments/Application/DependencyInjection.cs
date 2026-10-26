using SimplifiedPayments.Application.UseCases.Users;

namespace SimplifiedPayments.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services
    )
    {
        services.AddScoped<CreateUserHandler>();
        return services;
    }
}
