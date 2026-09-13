using AtelierTest.Manager;
using AtelierTest.Repositories.Model;

namespace AtelierTests;

[TestClass]
public sealed class StatisticsTests
{
    private static PlayerModel CreatePlayer(string countryCode, int height, int weightGrams, List<int> last)
    {
        return new PlayerModel
        {
            FirstName = "First",
            LastName = "Last",
            ShortName = "F.LAS",
            Sex = "M",
            Country = new Country { Code = countryCode },
            Data = new Data
            {
                Rank = 1,
                Points = 0,
                Weight = weightGrams,
                Height = height,
                Age = 30,
                Last = last,
            },
        };
    }

    [TestMethod]
    public void GetCountryWithBestWinRatio_AggregatesAcrossAllPlayersOfACountry()
    {
        // USA has two players; the aggregate ratio (3 wins / 6 matches = 0.5) must win out
        // over naively averaging each player's individual ratio.
        var players = new List<PlayerModel>
        {
            CreatePlayer("SRB", 188, 80000, [1, 1, 1, 1, 1]), // 5/5 = 1.0
            CreatePlayer("USA", 185, 74000, [0, 1, 0, 0, 1]), // 2/5
            CreatePlayer("USA", 175, 72000, [0, 1, 1, 1, 0]), // 3/5
        };

        var (country, ratio) = PlayerManager.GetCountryWithBestWinRatio(players);

        Assert.AreEqual("SRB", country);
        Assert.AreEqual(1.0, ratio);
    }

    [TestMethod]
    public void GetCountryWithBestWinRatio_OnReferenceDataset_ReturnsSrb()
    {
        var players = new List<PlayerModel>
        {
            CreatePlayer("SRB", 188, 80000, [1, 1, 1, 1, 1]), // 5/5 = 1.0
            CreatePlayer("USA", 185, 74000, [0, 1, 0, 0, 1]), // 2/5
            CreatePlayer("SUI", 183, 81000, [1, 1, 1, 0, 1]), // 4/5 = 0.8
            CreatePlayer("USA", 175, 72000, [0, 1, 1, 1, 0]), // 3/5
            CreatePlayer("ESP", 185, 85000, [1, 0, 0, 0, 1]), // 2/5
        };

        var (country, ratio) = PlayerManager.GetCountryWithBestWinRatio(players);

        Assert.AreEqual("SRB", country);
        Assert.AreEqual(1.0, ratio);
    }

    [TestMethod]
    public void GetCountryWithBestWinRatio_EmptyCollection_ReturnsNull()
    {
        var (country, ratio) = PlayerManager.GetCountryWithBestWinRatio([]);

        Assert.IsNull(country);
        Assert.IsNull(ratio);
    }

    [TestMethod]
    public void GetAverageBmi_AveragesIndividualBmisNotAverageWeightAndHeight()
    {
        var players = new List<PlayerModel>
        {
            CreatePlayer("SRB", 188, 80000, []), // 80 / 1.88^2 = 22.6358...
            CreatePlayer("USA", 175, 72000, []), // 72 / 1.75^2 = 23.5102...
        };

        var average = PlayerManager.GetAverageBmi(players);

        Assert.IsNotNull(average);
        Assert.AreEqual(23.073, average!.Value, 0.001);
    }

    [TestMethod]
    public void GetAverageBmi_OnReferenceDataset_MatchesExpectedValue()
    {
        var players = new List<PlayerModel>
        {
            CreatePlayer("SRB", 188, 80000, []),
            CreatePlayer("USA", 185, 74000, []),
            CreatePlayer("SUI", 183, 81000, []),
            CreatePlayer("USA", 175, 72000, []),
            CreatePlayer("ESP", 185, 85000, []),
        };

        var average = PlayerManager.GetAverageBmi(players);

        Assert.IsNotNull(average);
        Assert.AreEqual(23.36, average!.Value, 0.01);
    }

    [TestMethod]
    public void GetAverageBmi_EmptyCollection_ReturnsNull()
    {
        Assert.IsNull(PlayerManager.GetAverageBmi([]));
    }

    [TestMethod]
    public void GetMedianHeight_OddCount_ReturnsMiddleValue()
    {
        var players = new List<PlayerModel>
        {
            CreatePlayer("SRB", 188, 80000, []),
            CreatePlayer("USA", 185, 74000, []),
            CreatePlayer("SUI", 183, 81000, []),
            CreatePlayer("USA", 175, 72000, []),
            CreatePlayer("ESP", 185, 85000, []),
        };

        var median = PlayerManager.GetMedianHeight(players);

        Assert.AreEqual(185.0, median);
    }

    [TestMethod]
    public void GetMedianHeight_EvenCount_ReturnsAverageOfMiddleTwo()
    {
        var players = new List<PlayerModel>
        {
            CreatePlayer("A", 170, 70000, []),
            CreatePlayer("B", 180, 70000, []),
            CreatePlayer("C", 190, 70000, []),
            CreatePlayer("D", 200, 70000, []),
        };

        var median = PlayerManager.GetMedianHeight(players);

        Assert.AreEqual(185.0, median);
    }

    [TestMethod]
    public void GetMedianHeight_EmptyCollection_ReturnsNull()
    {
        Assert.IsNull(PlayerManager.GetMedianHeight([]));
    }

    [TestMethod]
    public void BuildStatistics_EmptyCollection_ReturnsAllNulls()
    {
        var stats = PlayerManager.BuildStatistics([]);

        Assert.IsNull(stats.BestWinRatioCountry);
        Assert.IsNull(stats.BestWinRatio);
        Assert.IsNull(stats.AverageBmi);
        Assert.IsNull(stats.MedianHeight);
    }
}
