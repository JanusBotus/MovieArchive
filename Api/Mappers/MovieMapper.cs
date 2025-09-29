using Api.Data;
using Api.Models.Database;
using Contracts.Dtos.Create;
using Contracts.Dtos.Read;
using Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Mappers;

/// <summary>
/// Mapper um MovieDto <-> MovieEntität zu mappen.
/// </summary>
public static class MovieMapper
{

    /// <summary>
    /// Erstellt aus einem Movie DTO eine Movie Entität.
    /// Falls Personen oder Genres in der Datenbank noch nicht vorhanden sind werden sie und die dazugehörigen Tabellen angelegt.
    /// </summary>
    /// <param name="dto">Quelldaten</param>
    /// <param name="_context">EF-Core Datenbankkontext</param>
    /// <param name="ct">Abbruchtoken</param>
    /// <returns>Die neu erzeugte Movie Entiät</returns>
    /// <exception cref="ArgumentNullException">DTO ist null</exception>
    /// <exception cref="ArgumentException">Altersfreigabe oder Veröffentlichungsdatum fehlen</exception>
    public static async Task<Movie> ToEntity(this MovieCreateDto dto, MovieArchiveDbContext _context, CancellationToken ct)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (!dto.AgeRating.HasValue)
        {
            throw new ArgumentException("Keine Altersfreigabe angegeben!");
        }

        if (!dto.ReleaseDate.HasValue)
        {
            throw new ArgumentException("Kein Erscheinungsdatum angegeben!");
        }

        Movie movie = new Movie
        {
            Title = dto.Title.Trim(),
            Rating = dto.Rating,
            Plot = dto.Plot,
            Runtime = dto.Runtime,
            Budget = dto.Budget,
            ReleaseDate = dto.ReleaseDate.Value,
            AgeRating = dto.AgeRating.Value
        };

        movie.MovieGenres = new List<MovieGenre>();
        foreach (var genre in dto.Genres)
        {
            var genreEntity = await _context.Genres.FirstOrDefaultAsync(g => g.Name == genre.Name, ct);
            if (genreEntity == null)
            {
                genreEntity = new Genre { Name = genre.Name };
                _context.Genres.Add(genreEntity);
            }

            movie.MovieGenres.Add(new MovieGenre
            {
                Movie = movie,
                Genre = genreEntity
            });
        }

        movie.MovieRoles = new List<PersonMovieRole>();
        foreach (var person in dto.Involvements)
        {
            var personEntity = await _context.People.FirstOrDefaultAsync(p => p.FirstName == person.FirstName && p.LastName == person.LastName, ct);
            if (personEntity == null)
            {
                personEntity = new Person
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName
                };
                _context.People.Add(personEntity);
            }

            foreach (var role in person.Roles.Distinct())
            {
                var roleEntity = await _context.Roles.FirstAsync(r => r.Id == (int)role, ct);
                movie.MovieRoles.Add(new PersonMovieRole
                {
                    Movie = movie,
                    Person = personEntity,
                    Role = roleEntity
                });
            }
        }

        return movie;
    }

    /// <summary>
    /// Mappt eine Movie Entität auf ein Movie DTO
    /// </summary>
    /// <param name="movie">Quelldaten</param>
    /// <returns>Eine MovieReadDto für das zurückschicken an den Client, mit IDs für Film, Person und Genre um Folgeaufrufe einfacher zu gestalten</returns>
    public static MovieReadDto ToDto(this Movie movie)
    {
        MovieReadDto dto = new MovieReadDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Rating = movie.Rating,
            ReleaseDate = movie.ReleaseDate,
            AgeRating = movie.AgeRating,
            Plot = movie.Plot,
            Runtime = movie.Runtime,
            Budget = movie.Budget ?? 0m,
            Genres = movie.MovieGenres
                .Select(mg => new GenreReadDto { Id = mg.Genre.Id, Name = mg.Genre.Name }).ToList(),
            Involvements = movie.MovieRoles
                .GroupBy(mr => new { mr.PersonId, mr.Person.FirstName, mr.Person.LastName })
                .Select(g => new PersonReadDto
                {
                    Id = g.Key.PersonId,
                    FirstName = g.Key.FirstName,
                    LastName = g.Key.LastName,
                    Roles = g.Select(mr => (MovieRole)mr.RoleId).Distinct().ToList()
                })
                .ToList()
        };

        return dto;
    }

}
