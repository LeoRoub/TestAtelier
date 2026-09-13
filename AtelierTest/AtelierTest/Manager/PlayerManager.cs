using AtelierTest.Controllers.Dto;
using AtelierTest.Manager.Interfaces;
using AtelierTest.Repositories.Interfaces;
using AtelierTest.Repositories.Model;

namespace AtelierTest.Manager;

public class PlayerManager : IPlayerManager
{
    private readonly IPlayerRepository _repository;

    public PlayerManager(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PlayerDto>> GetPlayersAsync()
    {
        var players = await _repository.GetAllAsync();

        return players
            .OrderBy(p => p.Data.Rank)
            .Select(ToDto)
            .ToList();
    }

    public async Task<PlayerDto?> GetPlayerByIdAsync(int id)
    {
        var player = await _repository.GetByIdAsync(id);
        return player is null ? null : ToDto(player);
    }

    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        var players = await _repository.GetAllAsync();
        return BuildStatistics(players);
    }

    public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto dto)
    {
        var model = new PlayerModel
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            ShortName = dto.ShortName,
            Sex = dto.Sex,
            Picture = dto.Picture,
            Country = new Country
            {
                Code = dto.Country.Code,
                Picture = dto.Country.Picture,
            },
            Data = new Data
            {
                Rank = dto.Data.Rank,
                Points = dto.Data.Points,
                Weight = dto.Data.Weight,
                Height = dto.Data.Height,
                Age = dto.Data.Age,
                Last = dto.Data.Last ?? [],
            },
        };

        var created = await _repository.AddAsync(model);
        return ToDto(created);
    }

    /// <summary>
    /// Calcule les statistiques agrégées sur un ensemble de joueurs. Extrait en méthode
    /// statique pure pour être testable indépendamment de toute base de données.
    /// </summary>
    internal static StatisticsDto BuildStatistics(IReadOnlyCollection<PlayerModel> players)
    {
        if (players.Count == 0)
        {
            return new StatisticsDto
            {
                BestWinRatioCountry = null,
                BestWinRatio = null,
                AverageBmi = null,
                MedianHeight = null,
            };
        }

        var (bestCountry, bestRatio) = GetCountryWithBestWinRatio(players);

        return new StatisticsDto
        {
            BestWinRatioCountry = bestCountry,
            BestWinRatio = bestRatio,
            AverageBmi = GetAverageBmi(players),
            MedianHeight = GetMedianHeight(players),
        };
    }

    internal static (string? Country, double? Ratio) GetCountryWithBestWinRatio(IReadOnlyCollection<PlayerModel> players)
    {
        if (players.Count == 0)
        {
            return (null, null);
        }

        // Agrégation au niveau pays (somme des victoires / somme des matchs joués),
        // et non moyenne des ratios individuels, pour ne pas sur-pondérer un pays
        // qui compte plusieurs joueurs.
        var byCountry = players
            .GroupBy(p => p.Country.Code)
            .Select(g => new
            {
                Country = g.Key,
                Wins = g.Sum(p => p.Data.Last.Count(result => result == 1)),
                Total = g.Sum(p => p.Data.Last.Count),
            })
            .Where(g => g.Total > 0)
            .Select(g => new { g.Country, Ratio = (double)g.Wins / g.Total })
            .OrderByDescending(g => g.Ratio)
            .ToList();

        if (byCountry.Count == 0)
        {
            return (null, null);
        }

        var best = byCountry[0];
        return (best.Country, best.Ratio);
    }

    internal static double? GetAverageBmi(IReadOnlyCollection<PlayerModel> players)
    {
        if (players.Count == 0)
        {
            return null;
        }

        // weight est exprimé en grammes, height en centimètres.
        return players.Average(p =>
        {
            var weightKg = p.Data.Weight / 1000.0;
            var heightM = p.Data.Height / 100.0;
            return weightKg / (heightM * heightM);
        });
    }

    internal static double? GetMedianHeight(IReadOnlyCollection<PlayerModel> players)
    {
        if (players.Count == 0)
        {
            return null;
        }

        var heights = players.Select(p => p.Data.Height).OrderBy(h => h).ToList();
        var mid = heights.Count / 2;

        return heights.Count % 2 == 0
            ? (heights[mid - 1] + heights[mid]) / 2.0
            : heights[mid];
    }

    private static PlayerDto ToDto(PlayerModel model)
    {
        return new PlayerDto
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            ShortName = model.ShortName,
            Sex = model.Sex,
            Picture = model.Picture,
            Country = new CountryDto
            {
                Code = model.Country.Code,
                Picture = model.Country.Picture,
            },
            Data = new DataDto
            {
                Rank = model.Data.Rank,
                Points = model.Data.Points,
                Weight = model.Data.Weight,
                Height = model.Data.Height,
                Age = model.Data.Age,
                Last = model.Data.Last,
            },
        };
    }
}
