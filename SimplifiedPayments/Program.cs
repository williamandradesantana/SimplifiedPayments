using SimplifiedPayments.Application;
using SimplifiedPayments.Configurations;
using SimplifiedPayments.Infrastructure;
using SimplifiedPayments.Infrastructure.Persistence;
using SimplifiedPayments.Middlewares;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddSerilogLogging();

builder.Services.AddEvolveConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddSqlServerConfiguration(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter(allowIntegerValues: false))
    );

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCorsConfiguration(builder.Configuration);
app.UseAuthorization();
app.MapControllers();

app.Run();
