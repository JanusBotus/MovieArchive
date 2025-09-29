using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.Dtos.Create;

/// <summary>
/// Schreibseitiges Datenübertragungsobjekt (DTO) zur Erstellung eines Genres.
/// </summary>
public class GenreCreateDto
{
    [Required(ErrorMessage = "Der Name des Genres muss angegeben werden!")]
    [StringLength(200)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Der Name eines Genres darf nicht leer sein oder nur aus Leerzeichen bestehen.")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

}
