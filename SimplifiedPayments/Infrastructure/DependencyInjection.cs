using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Infrastructure.Persistence;
using SimplifiedPayments.Infrastructure.Persistence.Repositories;
using SimplifiedPayments.Security;

namespace SimplifiedPayments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        return services;
    }
}
