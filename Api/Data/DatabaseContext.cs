using Microsoft.EntityFrameworkCore;
using Api.Models.Database;


namespace Api.Data
{

    /// <summary>
    /// Entity Framework Core Datenbank Kontext, stellt Entitäten und deren Beziehungen bereit.
    /// </summary>
    public class MovieArchiveDbContext : DbContext
    {
        public MovieArchiveDbContext(DbContextOptions<MovieArchiveDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; } = null!;

        public DbSet<Genre> Genres { get; set; } = null!;

        public DbSet<Person> People { get; set; } = null!;

        public DbSet<Role> Roles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /// Konfigurieren der "NormalizedTitle" Spalte, für das Abgleichen von eingegebenen Filmtiteln.
            modelBuilder.Entity<Movie>()
                .Property(m => m.NormalizedTitle)
                .HasComputedColumnSql("LOWER(REPLACE([TITLE], ' ', ''))", stored: true);

            modelBuilder.Entity<Movie>()
                .HasIndex(m => new { m.NormalizedTitle, m.ReleaseDate })
                .IsUnique();

            /// Tabelle: MovieGenre
            modelBuilder.Entity<MovieGenre>().HasKey(mg => new { mg.MovieId, mg.GenreId });

            modelBuilder.Entity<MovieGenre>()
                .HasOne(x => x.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(x => x.MovieId);

            modelBuilder.Entity<MovieGenre>()
                .HasOne(x => x.Genre)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(x => x.GenreId);

            /// Tabelle: PersonMovieRole
            modelBuilder.Entity<PersonMovieRole>().HasKey(pmr => new { pmr.PersonId, pmr.MovieId, pmr.RoleId });

            modelBuilder.Entity<PersonMovieRole>()
                .HasOne(x => x.Person)
                .WithMany(m => m.MovieRoles)
                .HasForeignKey(x => x.PersonId);

            modelBuilder.Entity<PersonMovieRole>()
                .HasOne(x => x.Movie)
                .WithMany(m => m.MovieRoles)
                .HasForeignKey(x => x.MovieId);

            modelBuilder.Entity<PersonMovieRole>()
                .HasOne(x => x.Role)
                .WithMany(m => m.MovieRoles)
                .HasForeignKey(x => x.RoleId);

            /// Role Seed-Daten
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasData(
                    new Role { Id = 1, Name = "LeadActor" },
                    new Role { Id = 2, Name = "Director" },
                    new Role { Id = 3, Name = "Writer" }
                );
            });

        }

    }
}