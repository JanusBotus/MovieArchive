using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Contracts.Enums;

namespace Contracts.Dtos.Read;

/// <summary>
/// Repräsentiert eine leseseitige Datenübertragungsstruktur (DTO) für eine Person,
/// inklusive Identifikator, Namen und Filmrollen.
/// </summary>
public class PersonReadDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vorname muss angegeben werden!")]
    [StringLength(200)]
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nachname muss angegeben werden!")]
    [StringLength(200)]
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [MinLength(1)]
    [JsonPropertyName("roles")]
    public List<MovieRole> Roles { get; set; } = new List<MovieRole>();
}
