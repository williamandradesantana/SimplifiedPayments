using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace SimplifiedPayments.Tests.Integration;

public sealed class ApiFixture : IAsyncLifetime
{
    public const string AllowedOrigin = "http://localhost:3000";

    private static readonly string[] EnvironmentKeys =
        ["ConnectionStrings__SqlServer", "Cors__AllowedOrigins__0"];

    private readonly MsSqlContainer _sql =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _factory!.Services;

    public async Task InitializeAsync()
    {
        await _sql.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(_sql.GetConnectionString())
        {
            InitialCatalog = "simplified_payments_tests"
        }.ConnectionString;

        Environment.SetEnvironmentVariable("ConnectionStrings__SqlServer", connectionString);
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins__0", AllowedOrigin);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));

        Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            AllowAutoRedirect = false
        });
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (_factory is not null) await _factory.DisposeAsync();

        await _sql.DisposeAsync();

        foreach (var key in EnvironmentKeys) 
            Environment.SetEnvironmentVariable(key, null);
    }

}
