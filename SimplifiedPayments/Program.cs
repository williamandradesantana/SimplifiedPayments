using SimplifiedPayments.Configurations;
using SimplifiedPayments.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddSerilogLogging();

builder.Services.AddEvolveConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddSqlServerConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCorsConfiguration();
app.UseAuthorization();

app.MapControllers();

app.Run();
