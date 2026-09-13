using System.Text.Json;
using System.Text.Json.Serialization;
using AtelierTest.Repositories.Model;
using Microsoft.EntityFrameworkCore;

namespace AtelierTest.Repositories;

public static class PlayerSeeder
{
    public static async Task SeedAsync(PlayerContext context, string jsonFilePath)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Players.AnyAsync())
        {
            return;
        }

        if (!File.Exists(jsonFilePath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var payload = JsonSerializer.Deserialize<SeedPayload>(json, options);

        if (payload?.Players is null || payload.Players.Count == 0)
        {
            return;
        }

        context.Players.AddRange(payload.Players);
        await context.SaveChangesAsync();
    }

    private sealed class SeedPayload
    {
        [JsonPropertyName("players")]
        public List<PlayerModel>? Players { get; set; }
    }
}
