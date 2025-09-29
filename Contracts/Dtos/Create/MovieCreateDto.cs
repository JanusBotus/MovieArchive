using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Contracts.Enums;

namespace Contracts.Dtos.Create;

/// <summary>
/// Schreibseitiges Datenübertragungsobjekt (DTO) zur Erstellung eines Films,
/// inklusive Stammdaten, Bewertung, Altersfreigabe, Beteiligungen und Genres.
/// </summary>
public class MovieCreateDto
{
    [Required(ErrorMessage = "Filmtitel ist erforderlich!")]
    [StringLength(200)]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [Range(0.0, 10.0, ErrorMessage = "Bewertung muss zwischen 0 und 10 liegen!")]
    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [Required(ErrorMessage = "Erscheinungsdatum (zwischen 1895 und 2100) ist erforderlich!")]
    [Range(typeof(DateOnly), "1895-01-01", "2100-12-31")]
    [JsonPropertyName("releaseDate")]
    public DateOnly? ReleaseDate { get; set; }

    [Required(ErrorMessage = "Es muss eine Alterfreigabe angegeben werden!")]
    [JsonPropertyName("ageRating")]
    public AgeRating? AgeRating { get; set; }

    [Required(ErrorMessage = "Es muss eine Handlung angegeben werden!")]
    [StringLength(10000)]
    [JsonPropertyName("plot")]
    public string Plot { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Die Laufzeit muss >= 0 sein!")]
    [JsonPropertyName("runtime")]
    public int Runtime { get; set; }

    [Range(typeof(decimal), "0", "100000000000", ErrorMessage = "Das Budget muss >= 0 sein!")]
    [JsonPropertyName("budget")]
    public decimal Budget { get; set; }

    [MinLength(1, ErrorMessage = "Es muss mindestens eine Beteiligung geben!")]
    [JsonPropertyName("involvements")]
    public List<PersonCreateDto> Involvements { get; set; } = new List<PersonCreateDto>();

    [MinLength(1, ErrorMessage = "Es muss mindestens ein Genre angegeben sein!")]
    [JsonPropertyName("genres")]
    public List<GenreCreateDto> Genres { get; set; } = new List<GenreCreateDto>();
}
