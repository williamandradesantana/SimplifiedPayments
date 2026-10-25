using EvolveDb;
using Microsoft.Data.SqlClient;
using Serilog;

namespace SimplifiedPayments.Infrastructure.Persistence;

public static class EvolveConfiguration
{
    public static IServiceCollection AddEvolveConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment
    )
    {
        if (!environment.IsDevelopment())
            return services;

        var connectionString = configuration.GetConnectionString("SqlServer");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'SqlServer' is not configured.");

        try
        {
            EnsureDatabaseExists(connectionString);
            ExecuteMigrations(connectionString);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the database.");
            throw;
        }

        return services;
    }

    private static void ExecuteMigrations(string connectionString)
    {
        using var evolveConnection = new SqlConnection(connectionString);
        var evolve = new Evolve(evolveConnection, message => Log.Information("{message}", message))
        {
            Locations = [Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Persistence", "Migrations")],
            IsEraseDisabled = true
        };
        evolve.Migrate();
    }

    private static void EnsureDatabaseExists(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "IF DB_ID(@name) IS NULL EXEC('CREATE DATABASE ' + QUOTENAME(@name))";
        command.Parameters.AddWithValue("@name", databaseName);
        command.ExecuteNonQuery();
    }
}
