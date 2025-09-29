using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Database
{

    /// <summary>
    ///  Join-Entität, die einen Film einem Genre zuordnet.
    /// </summary>
    
    [Table("MovieGenres")]
    public class MovieGenre
    {
        public int MovieId { get; set; }
        public int GenreId { get; set; }

        public Movie Movie { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }
}
