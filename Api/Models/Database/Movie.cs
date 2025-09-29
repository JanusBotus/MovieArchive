using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;


namespace Api.Models.Database
{

    /// <summary>
    /// Repräsentiert einen Film mit Metadaten.
    /// </summary>

    [Table("Movies")]
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Range(0, 10)]
        public double Rating { get; set; }

        public DateOnly ReleaseDate { get; set; }

        public AgeRating AgeRating { get; set; }

        [Required]
        public string Plot { get; set; } = string.Empty;

        public int Runtime { get; set; }

        public decimal? Budget { get; set; }

        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

        public ICollection<PersonMovieRole> MovieRoles { get; set; } = new List<PersonMovieRole>();

        public string NormalizedTitle { get; private set; } = string.Empty;

    }
}
