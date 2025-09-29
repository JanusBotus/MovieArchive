using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.Dtos.Read;

/// <summary>
/// Repräsentiert eine leseseitige Datenübertragungsstruktur (DTO) für ein Filmgenre.
/// </summary>
public class GenreReadDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "Der Name des Genres muss angegeben werden!")]
    [StringLength(200)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

}
