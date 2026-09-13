namespace AtelierTest.Repositories.Model;

public class PlayerModel
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string ShortName { get; set; }
    public required string Sex { get; set; }
    public string? Picture { get; set; }
    public required Country Country { get; set; }
    public required Data Data { get; set; }
}

public class Country
{
    public string? Picture { get; set; }
    public required string Code { get; set; }
}

public class Data
{
    public int Rank { get; set; }
    public int Points { get; set; }
    /// <summary>Poids en grammes, tel que fourni par la source de données.</summary>
    public int Weight { get; set; }
    /// <summary>Taille en centimètres.</summary>
    public int Height { get; set; }
    public int Age { get; set; }
    public List<int> Last { get; set; } = [];
}
