using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Database
{

    /// <summary>
    /// Repräsentiert eine Person im Film.
    /// </summary>

    [Table("People")]
    public class Person
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public ICollection<PersonMovieRole> MovieRoles { get; set; } = new List<PersonMovieRole>();
    }
}