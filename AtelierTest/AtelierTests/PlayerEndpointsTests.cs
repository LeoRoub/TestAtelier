using System.Net;
using System.Net.Http.Json;
using AtelierTest.Controllers.Dto;
using AtelierTest.Repositories;
using AtelierTest.Repositories.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AtelierTests;

/// <summary>
/// Swaps the app's SQLite file database for a single, shared in-memory SQLite
/// connection kept open for the lifetime of the factory, so the schema and any
/// seeded data survive across requests within a test.
/// </summary>
public sealed class PlayerApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public PlayerApiFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PlayerContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<PlayerContext>(options => options.UseSqlite(_connection));
        });
    }

    /// <summary>Ensures the database has a deterministic, known player for the tests to rely on.</summary>
    public async Task<PlayerModel> SeedKnownPlayerAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();

        var player = new PlayerModel
        {
            Id = 17,
            FirstName = "Rafael",
            LastName = "Nadal",
            ShortName = "R.NAD",
            Sex = "M",
            Country = new Country { Code = "ESP" },
            Data = new Data { Rank = 1, Points = 1982, Weight = 85000, Height = 185, Age = 33, Last = [1, 0, 0, 0, 1] },
        };

        if (!await context.Players.AnyAsync(p => p.Id == player.Id))
        {
            context.Players.Add(player);
            await context.SaveChangesAsync();
        }

        return player;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

[TestClass]
public sealed class PlayerEndpointsTests
{
    private static PlayerApiFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _factory = new PlayerApiFactory();
        _client = _factory.CreateClient();
        await _factory.SeedKnownPlayerAsync();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task GetPlayers_ReturnsOkWithAtLeastTheSeededPlayer()
    {
        var response = await _client.GetAsync("/api/players");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var players = await response.Content.ReadFromJsonAsync<List<PlayerDto>>();

        Assert.IsNotNull(players);
        Assert.IsTrue(players!.Any(p => p.Id == 17 && p.LastName == "Nadal"));
    }

    [TestMethod]
    public async Task GetPlayerById_KnownId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/players/17");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var player = await response.Content.ReadFromJsonAsync<PlayerDto>();

        Assert.IsNotNull(player);
        Assert.AreEqual("Nadal", player!.LastName);
    }

    [TestMethod]
    public async Task GetPlayerById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/players/999999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetStatistics_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/players/statistics");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var stats = await response.Content.ReadFromJsonAsync<StatisticsDto>();

        Assert.IsNotNull(stats);
        Assert.IsNotNull(stats!.AverageBmi);
        Assert.IsNotNull(stats.MedianHeight);
        Assert.IsNotNull(stats.BestWinRatioCountry);
    }

    [TestMethod]
    public async Task CreatePlayer_ValidPayload_ReturnsCreatedAndIsRetrievable()
    {
        var newPlayer = new CreatePlayerDto
        {
            FirstName = "New",
            LastName = "Player",
            ShortName = "N.PLA",
            Sex = "M",
            Country = new CreateCountryDto { Code = "FRA" },
            Data = new CreateDataDto { Rank = 500, Points = 10, Weight = 78000, Height = 180, Age = 22, Last = [1, 0] },
        };

        var response = await _client.PostAsJsonAsync("/api/players", newPlayer);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.IsNotNull(created);
        Assert.AreEqual("Player", created!.LastName);

        var getResponse = await _client.GetAsync(response.Headers.Location);
        Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [TestMethod]
    public async Task CreatePlayer_MissingRequiredField_ReturnsBadRequest()
    {
        var payload = new
        {
            firstname = "Missing",
            // lastname intentionally omitted
            shortname = "M.ISS",
            sex = "M",
            country = new { code = "FRA" },
            data = new { rank = 1, points = 0, weight = 70000, height = 180, age = 25, last = new int[0] },
        };

        var response = await _client.PostAsJsonAsync("/api/players", payload);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
