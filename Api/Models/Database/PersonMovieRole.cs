using System.ComponentModel.DataAnnotations.Schema;


namespace Api.Models.Database
{

    /// <summary>
    /// Join-Entität, die eine Person zu einer bestimmten Rolle in einem bestimmten Film zuordnet. 
    /// </summary>

    [Table("PersonMovieRoles")]
    public class PersonMovieRole
    {
        public int MovieId { get; set; }
        public int PersonId { get; set; }
        public int RoleId { get; set; }

        public Movie Movie { get; set; } = null!;
        public Person Person { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
