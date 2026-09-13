using AtelierTest.Controllers.Dto;
using AtelierTest.Manager;
using AtelierTest.Repositories.Interfaces;
using AtelierTest.Repositories.Model;

namespace AtelierTests;

[TestClass]
public sealed class PlayerManagerTests
{
    private sealed class FakePlayerRepository : IPlayerRepository
    {
        public List<PlayerModel> Players { get; } = [];

        public Task<IReadOnlyList<PlayerModel>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<PlayerModel>>(Players.ToList());
        }

        public Task<PlayerModel?> GetByIdAsync(int id)
        {
            return Task.FromResult(Players.FirstOrDefault(p => p.Id == id));
        }

        public Task<PlayerModel> AddAsync(PlayerModel player)
        {
            player.Id = Players.Count == 0 ? 1 : Players.Max(p => p.Id) + 1;
            Players.Add(player);
            return Task.FromResult(player);
        }
    }

    private static PlayerModel CreatePlayer(int id, string countryCode, int rank)
    {
        return new PlayerModel
        {
            Id = id,
            FirstName = "First",
            LastName = "Last",
            ShortName = "F.LAS",
            Sex = "M",
            Country = new Country { Code = countryCode },
            Data = new Data { Rank = rank, Points = 0, Weight = 80000, Height = 185, Age = 30, Last = [] },
        };
    }

    [TestMethod]
    public async Task GetPlayersAsync_ReturnsPlayersSortedByRankAscending()
    {
        var repository = new FakePlayerRepository();
        repository.Players.AddRange([
            CreatePlayer(1, "USA", 52),
            CreatePlayer(2, "SRB", 2),
            CreatePlayer(3, "ESP", 1),
        ]);
        var manager = new PlayerManager(repository);

        var players = await manager.GetPlayersAsync();

        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, players.Select(p => p.Id).ToArray());
    }

    [TestMethod]
    public async Task GetPlayerByIdAsync_UnknownId_ReturnsNull()
    {
        var manager = new PlayerManager(new FakePlayerRepository());

        var player = await manager.GetPlayerByIdAsync(999);

        Assert.IsNull(player);
    }

    [TestMethod]
    public async Task GetPlayerByIdAsync_KnownId_ReturnsPlayer()
    {
        var repository = new FakePlayerRepository();
        repository.Players.Add(CreatePlayer(17, "ESP", 1));
        var manager = new PlayerManager(repository);

        var player = await manager.GetPlayerByIdAsync(17);

        Assert.IsNotNull(player);
        Assert.AreEqual(17, player!.Id);
    }

    [TestMethod]
    public async Task CreatePlayerAsync_AssignsIncrementedIdAndPersists()
    {
        var repository = new FakePlayerRepository();
        repository.Players.Add(CreatePlayer(17, "ESP", 1));
        var manager = new PlayerManager(repository);

        var dto = new CreatePlayerDto
        {
            FirstName = "New",
            LastName = "Player",
            ShortName = "N.PLA",
            Sex = "M",
            Country = new CreateCountryDto { Code = "FRA" },
            Data = new CreateDataDto { Rank = 100, Points = 10, Weight = 75000, Height = 180, Age = 25, Last = [1] },
        };

        var created = await manager.CreatePlayerAsync(dto);

        Assert.AreEqual(18, created.Id);
        Assert.AreEqual(2, repository.Players.Count);
        Assert.AreEqual("FRA", repository.Players.Last().Country.Code);
    }
}
