using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Infrastructure.ExternalServices;
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
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddExternalServices();
        return services;
    }
}
