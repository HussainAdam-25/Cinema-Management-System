using Cinema_BusinessLayer;
using Cinema_BusinessLayer.Models;
using Cinema_DataAccessLayer.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagementSystem.Controllers
{
    [Authorize] // يحمي الكل
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public MoviesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ====== READ ====== //

        [HttpGet("All-Movies")]
        [ProducesResponseType(typeof(IEnumerable<Hall>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MovieReadDto>>> GetAllMovies(
            [FromQuery] string? search,
            [FromQuery] string? genre,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var q = _unitOfWork.Movies.QueryNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(m => m.Title.Contains(search));

            if (!string.IsNullOrWhiteSpace(genre))
                q = q.Where(m => m.Genre != null && m.Genre == genre);

            var items = await q
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovieReadDto(m.Id, m.Title, m.Genre, m.DurationMinutes, m.ReleaseDate))
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("Find-By-Id/{id:int}")]
        [ProducesResponseType(typeof(Hall), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MovieReadDto>> FindById(int id)
        {
            var m = await _unitOfWork.Movies.GetByIdAsync(id);
            if (m == null)
                return NotFound();
            return new MovieReadDto(m.Id, m.Title, m.Genre, m.DurationMinutes, m.ReleaseDate);
        }

        [HttpGet("Find-By-Title")]
        [ProducesResponseType(typeof(IEnumerable<Movie>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Movie>> FindByTitle(string title)
        {
            var movie = await _unitOfWork.Movies.FindAsync(h => h.Title == title);
            if (movie == null)
                return NotFound();
            return movie;
        }

        // ====== CREATE ====== //

        [HttpPost("Add-New-Movie")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MovieReadDto>> CreateNewMovie([FromBody] CreateMovieDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid movie data.");

            var title = dto.Title?.Trim();

            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Movie title is required.");

            // التحقق من التكرار
            var exists = await _unitOfWork.Movies.AnyAsync(h => h.Title.ToLower() == title.ToLower());
            if (exists)
                return Conflict("A moviw with this title is already exists.");

            var entity = new Movie
            {
                Title = dto.Title,
                Genre = dto.Genre,
                DurationMinutes = dto.DurationMinutes,
                ReleaseDate = dto.ReleaseDate
            };

            var created = await _unitOfWork.Movies.AddAsync(entity);

            var id = created.Id; _unitOfWork.Complete();

            return CreatedAtAction(nameof(FindById), new { id }, created);
        }

        // ====== PATCH ====== //

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<MovieReadDto>> PatchMovie(int id, [FromBody] Dictionary<string, object> changes)
        {
            var m = await _unitOfWork.Movies.FindAsync(x => x.Id == id);
            if (m is null) return NotFound();

            // تطبيق تغييرات بسيطة وآمنة
            foreach (var (key, val) in changes)
            {
                switch (key.ToLowerInvariant())
                {
                    case "title": m.Title = Convert.ToString(val) ?? m.Title; break;
                    case "genre": m.Genre = val is null ? null : Convert.ToString(val); break;
                    case "durationminutes":
                        if (int.TryParse(Convert.ToString(val), out var d) && d > 0) m.DurationMinutes = d;
                        break;
                    case "releasedate":
                        if (DateTime.TryParse(Convert.ToString(val), out var dt)) m.ReleaseDate = dt;
                        break;
                }
            }

            _unitOfWork.Complete();

            return new MovieReadDto(m.Id, m.Title, m.Genre, m.DurationMinutes, m.ReleaseDate);
        }

        // ====== UPDATE ====== //

        [HttpPut("Update-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateById(int id, [FromBody] UpdateMovieDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid movie data.");

            var movie = await _unitOfWork.Movies.GetByIdAsync(id);
            if (movie == null)
                return NotFound("This Movie is not found.");

            var name = dto.Title?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Movie title is required.");

            var exists = await _unitOfWork.Movies.AnyAsync(h => h.Id != id && h.Title.ToLower() == name.ToLower());
            if (exists)
                return Conflict("Another movie with this title is already exists.");

            var m = await _unitOfWork.Movies.FindAsync(i => i.Id == id);
            if (m is null) return NotFound();

            movie.Title = dto.Title;
            movie.Genre = dto.Genre;
            movie.DurationMinutes = dto.DurationMinutes;
            movie.ReleaseDate = dto.ReleaseDate;

            _unitOfWork.Movies.Update(movie);
            _unitOfWork.Complete();

            return NoContent();
        }

        [HttpPut("Update-By-Name")]
        public async Task<IActionResult> UpdateByName([FromQuery] string title, [FromBody] UpdateMovieDto dto)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Movie title is required.");

            var movie = await _unitOfWork.Movies
                .Query()
                .FirstOrDefaultAsync(h => h.Title.ToLower() == title.ToLower());

            if (movie == null)
                return NotFound($"No movie found with the title '{title}'.");

            // تحديث القيم
            movie.Genre = dto.Genre;
            movie.DurationMinutes = dto.DurationMinutes;
            movie.ReleaseDate = dto.ReleaseDate;

            _unitOfWork.Movies.Update(movie);
            _unitOfWork.Complete();

            return NoContent();
        }

        // ====== DELETE ====== //

        [HttpDelete("Delete-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(int id)
        {
            var movie = await _unitOfWork.Movies.FindAsync(h => h.Id == id);
            if (movie == null)
                return NotFound(new { message = "No movie found with this Id" });

            var deleted = await _unitOfWork.Movies.DeleteAsync(movie.Id);
            _unitOfWork.Complete();

            return Ok(new { message = $"This Movie has been successfully deleted." });
        }

        [HttpDelete("Delete-By-Title")]
        public async Task<IActionResult> DeleteByTitle(string title)
        {
            var movie = await _unitOfWork.Movies.FindAsync(h => h.Title == title);
            if (movie == null)
                return NotFound(new { message = "No movie found with this title" });

            var deleted = await _unitOfWork.Movies.DeleteAsync(movie.Id);
            _unitOfWork.Complete();

            return Ok(new { message = $"Movie '{title}' deleted successfully" });
        }
    }
}
