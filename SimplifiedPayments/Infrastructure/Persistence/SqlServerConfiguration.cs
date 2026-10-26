using Microsoft.EntityFrameworkCore;

namespace SimplifiedPayments.Infrastructure.Persistence;

public static class SqlServerConfiguration
{

    public static IServiceCollection AddSqlServerConfiguration(
        this IServiceCollection services, IConfiguration configuration
    )
    {
        var connectionString = configuration["ConnectionStrings:SqlServer"];
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException("Connection string for Sql Server is not configured!");
        
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        return services;
    }
}
