using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AtelierTest.Controllers.Dto;

public class PlayerDto
{
    public int Id { get; set; }

    [JsonPropertyName("firstname")]
    public required string FirstName { get; set; }

    [JsonPropertyName("lastname")]
    public required string LastName { get; set; }

    [JsonPropertyName("shortname")]
    public required string ShortName { get; set; }

    public required string Sex { get; set; }

    public string? Picture { get; set; }

    public required CountryDto Country { get; set; }

    public required DataDto Data { get; set; }
}

public class CountryDto
{
    public string? Picture { get; set; }
    public required string Code { get; set; }
}

public class DataDto
{
    public int Rank { get; set; }

    [JsonPropertyName("points")]
    public int Points { get; set; }

    /// <summary>Poids en grammes.</summary>
    public int Weight { get; set; }

    /// <summary>Taille en centimètres.</summary>
    public int Height { get; set; }

    public int Age { get; set; }

    public List<int> Last { get; set; } = [];
}

public class CreatePlayerDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    [JsonPropertyName("firstname")]
    public required string FirstName { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    [JsonPropertyName("lastname")]
    public required string LastName { get; set; }

    [Required, StringLength(20, MinimumLength = 1)]
    [JsonPropertyName("shortname")]
    public required string ShortName { get; set; }

    [Required, RegularExpression("^[MF]$", ErrorMessage = "Sex must be 'M' or 'F'.")]
    public required string Sex { get; set; }

    public string? Picture { get; set; }

    [Required]
    public required CreateCountryDto Country { get; set; }

    [Required]
    public required CreateDataDto Data { get; set; }
}

public class CreateCountryDto
{
    public string? Picture { get; set; }

    [Required, StringLength(10, MinimumLength = 2)]
    public required string Code { get; set; }
}

public class CreateDataDto
{
    [Range(1, int.MaxValue)]
    public int Rank { get; set; }

    [Range(0, int.MaxValue)]
    public int Points { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Weight must be expressed in grams and be greater than 0.")]
    public int Weight { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Height must be expressed in centimeters and be greater than 0.")]
    public int Height { get; set; }

    [Range(0, 150)]
    public int Age { get; set; }

    [BinaryResults]
    public List<int> Last { get; set; } = [];
}

public class StatisticsDto
{
    public string? BestWinRatioCountry { get; set; }
    public double? BestWinRatio { get; set; }
    public double? AverageBmi { get; set; }
    public double? MedianHeight { get; set; }
}

/// <summary>Valide que chaque entrée est 0 (défaite) ou 1 (victoire).</summary>
public sealed class BinaryResultsAttribute : ValidationAttribute
{
    public BinaryResultsAttribute()
    {
        ErrorMessage = "Each entry in Last must be 0 (loss) or 1 (win).";
    }

    public override bool IsValid(object? value)
    {
        return value is not List<int> results || results.All(result => result is 0 or 1);
    }
}
