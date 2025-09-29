using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Database
{

    /// <summary>
    /// Repräsentiert eine Rolle die eine Person innerhalb eines Films annehmen kann.
    /// </summary>

    [Table("Roles")]
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<PersonMovieRole> MovieRoles { get; set; } = new List<PersonMovieRole>();
    }
}