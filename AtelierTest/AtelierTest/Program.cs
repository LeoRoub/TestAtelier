using AtelierTest.Manager;
using AtelierTest.Manager.Interfaces;
using AtelierTest.Repositories;
using AtelierTest.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("PlayerDb") ?? "Data Source=players.db";
builder.Services.AddDbContext<PlayerContext>(options => options.UseSqlite(connectionString));

// Ensure the directory for the SQLite file exists (needed for the persistent
// /home/data mount on Azure App Service, which is not created automatically).
var sqliteBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
var sqliteDirectory = Path.GetDirectoryName(Path.GetFullPath(sqliteBuilder.DataSource));
if (!string.IsNullOrEmpty(sqliteDirectory))
{
    Directory.CreateDirectory(sqliteDirectory);
}

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IPlayerManager, PlayerManager>();

var app = builder.Build();

// Seed the database from the reference dataset on startup.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
    var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "headtohead.json");
    await PlayerSeeder.SeedAsync(context, dataPath);
}

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

if (app.Environment.IsDevelopment())
{
    // Swagger UI, servie en local à partir du document OpenAPI déjà généré ci-dessus.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "AtelierTest API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
