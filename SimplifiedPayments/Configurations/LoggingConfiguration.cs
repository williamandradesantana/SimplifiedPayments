using Serilog;

namespace SimplifiedPayments.Configurations;

public static class LoggingConfiguration
{
    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .CreateLogger();
        builder.Host.UseSerilog();
    }
}
