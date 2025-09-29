using Api.Data;
using Api.Mappers;
using Contracts.Dtos.Create;
using Contracts.Dtos.Read;
using Contracts.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Api.Controller
{

    /// <summary>
    /// Stellt Endpunkte zur Verwaltung von Filmen bereit (Erstellen, Abfragen, Autocomplete).
    /// Basisroute: movie-archive.
    /// </summary>
    [ApiController]
    [Route("movie-archive")]
    public class MoviesController : ControllerBase
    {
        private readonly MovieArchiveDbContext _context;

        public MoviesController(MovieArchiveDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Legt einen neuen Film in der Datenbank an.
        /// </summary>
        /// <param name="movieCreateDto">Eingabedaten.</param>
        /// <param name="ct">Abbruchtoken.</param>
        /// <returns>
        /// Gibt bei Erfolg MovieReadDto des neu angelegten Films zurück.
        /// Bei Misserfolg:
        /// 400 - Bei Validierungsfehlern
        /// 409 - Wenn der Film bereits existiert
        /// 422 - Bei Fremdschlüssel oder Contraintverletzungen
        /// 500 - Bei anderem Datenbankfehler
        /// </returns>
        [HttpPost("movies")]
        public async Task<ActionResult<MovieReadDto>> CreateMovie([FromBody] MovieCreateDto movieCreateDto, CancellationToken ct)
        {

            try
            {
                var entity = await movieCreateDto.ToEntity(_context, ct);

                _context.Movies.Add(entity);
                await _context.SaveChangesAsync(ct);
                var movieReadDto = entity.ToDto();
                return CreatedAtAction(nameof(GetMovieById), new { id = entity.Id }, movieReadDto);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqliteException se)
                {
                    if (se.SqliteExtendedErrorCode is 2067 or 1555 or 2579)
                    {
                        return Conflict("Film existiert bereits!");
                    }

                    if (se.SqliteExtendedErrorCode is 787 or 275 or 1299)
                    {
                        return UnprocessableEntity("Daten sind ungültig!");
                    }

                    if (se.SqliteExtendedErrorCode is 19)
                    {
                        return UnprocessableEntity("Es wurde ein Constraint verletzt!");
                    }
                }

                return Problem(title: "Datenbankfehler", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Liefert eine Liste aller Filme inklusive Genres und Mitwirkenden.
        /// </summary>
        /// <param name="ct">Abbruchtoken</param>
        /// <returns>Liste von MovieReadDtos</returns>
        [HttpGet("movies")]
        public async Task<ActionResult<List<MovieReadDto>>> GetAllMovies(CancellationToken ct)
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .Include(movie => movie.MovieGenres).ThenInclude(movieGenres => movieGenres.Genre)
                .Include(movie => movie.MovieRoles).ThenInclude(movieGenres => movieGenres.Person)
                .Include(movie => movie.MovieRoles).ThenInclude(movieGenres => movieGenres.Role)
                .ToListAsync(ct);

            var movieReadDtos = movies.Select(m => m.ToDto()).ToList();

            return Ok(movieReadDtos);
        }

        /// <summary>
        /// Liefert einen Film anhand seiner Id.
        /// </summary>
        /// <param name="id">Primärschlüssel (Id) des Films.</param>
        /// <param name="ct">Abbruchtoken.</param>
        /// <returns>Bei Erfolg MovieReadDto mit Daten des Films, bei Misserfolg 404 Not Found.</returns>
        [HttpGet("movie/by-id/{id:int}")]
        public async Task<ActionResult<MovieReadDto>> GetMovieById(int id, CancellationToken ct)
        {
            var movie = await _context.Movies
                .AsNoTracking()
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieRoles).ThenInclude(mr => mr.Person)
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (movie is null)
            {
                return NotFound();
            }
            return Ok(movie.ToDto());
        }

        /// <summary>
        /// Liefert einen Film anhand seines Titels.
        /// </summary>
        /// <param name="title">Nicht normalisierte Filmtitel.</param>
        /// <param name="ct">Abbruchtoken.</param>
        /// <returns>Bei Erfolg MovieReadDto mit Daten des Films, bei Misserfolg 400 Bad Request.</returns>
        [HttpGet("movie/by-title/{title}")]
        public async Task<ActionResult<MovieReadDto>> GetMovieByTitle(string title, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Titel darf nicht leer sein!");
            }

            var normalizedTitle = title.Replace(" ", "").ToLowerInvariant();

            var movie = await _context.Movies
                .AsNoTracking()
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieRoles).ThenInclude(mr => mr.Person)
                .FirstOrDefaultAsync(m => m.NormalizedTitle == normalizedTitle, ct);

            if (movie is null)
            {
                return NotFound();
            }

            return Ok(movie.ToDto());
        }

        /// <summary>
        /// Liefert alle Filme, in denen eine bestimmte Person eine bestimmte Rolle eingenommen hat.
        /// </summary>
        /// <param name="role">Die gesuchte Filmrolle (Hauptdarsteller, Regisseur, Drehbuchautor).</param>
        /// <param name="personId">ID der Person.</param>
        /// <param name="ct">Abbruchtoken.</param>
        /// <returns>Liste von MovieReadDtos, leere Liste falls es keinen Film zur gesuchten Person/Rolle Kombination gab.</returns>
        [HttpGet("movie/by-person-role")]
        public async Task<ActionResult<List<MovieReadDto>>> GetMoviesByPersonAndRole([FromQuery] MovieRole role, [FromQuery] int personId, CancellationToken ct)
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .Where(m => m.MovieRoles.Any(mr => mr.RoleId == (int)role && mr.PersonId == personId))
                .Include(m => m.MovieRoles).ThenInclude(mr => mr.Person)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .ToListAsync(ct);

            var moviesDtoList = movies.Select(m => m.ToDto()).ToList();

            return Ok(moviesDtoList);
        }


        /// <summary>
        /// Liefert alle Filme eines bestimmten Genres.
        /// </summary>
        /// <param name="genreId">ID des Genres.</param>
        /// <param name="ct">Abbruchtoken.</param>
        /// <returns>Liste von MovieReadDtos aller Filme des Genres.</returns>
        [HttpGet("genre/{genreId:int}")]
        public async Task<ActionResult<List<MovieReadDto>>> GetMoviesByGenre(int genreId, CancellationToken ct)
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId))
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieRoles).ThenInclude(mr => mr.Person)
                .ToListAsync(ct);

            var moviesDtoList = movies.Select(m => m.ToDto()).ToList();

            return Ok(moviesDtoList);
        }

        /// <summary>
        /// Liefert bis zu max. 10 Titelvorschlägen (Autocomplete) anhand einer Suchanfrage.
        /// </summary>
        /// <param name="query">Nicht normalisierter Suchstring.</param>
        /// <param name="ct">Abbruchtoken.</param>
        /// <returns>Liste von MovieReadDtos die den Suchstring beinhalten.</returns>
        [HttpGet("autocomplete/{query}")]
        public async Task<ActionResult<MovieReadDto>> GetAutocomplete(string query, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Anfrage darf nicht leer sein!");
            }

            var normalizedQuery = query.Replace(" ", "").ToLowerInvariant();

            var movies = await _context.Movies
                .AsNoTracking()
                .Where(m => m.NormalizedTitle.Contains(normalizedQuery))
                .OrderBy(m => m.Title)
                .Take(10)
                .ToListAsync(ct);

            var moviesDtoList = movies.Select(m => m.ToDto()).ToList();

            return Ok(moviesDtoList);
        }
    }
}