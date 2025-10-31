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
    public class ShowtimesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShowtimesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ====== READ ====== //

        [HttpGet("All-Showtimes")]
        [ProducesResponseType(typeof(IEnumerable<Showtime>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllShowtimes([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            if (take <= 0) take = 50;
            if (take > 200) take = 200;

            var data = await _unitOfWork.Showtimes.Query()   
                .OrderBy(st => st.StartsAt)
                .Skip(skip)
                .Take(take)
                .Select(st => new ReadShowtimeDto
                {
                    ShowtimeId = st.Id,
                    MovieTitle = st.Movie.Title, 
                    HallName = st.Hall.Name,  
                    StartsAtUtc = st.StartsAt,   
                    Price = st.Price
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("Find-By-Id/{id:int}")]
        [ProducesResponseType(typeof(Seat), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Showtime>> FindById(int id)
        {
            var showtime = await _unitOfWork.Showtimes.GetByIdAsync(id);
            if (showtime == null)
                return NotFound();
            return showtime;
        }

        // ====== CREATE ====== //

        [HttpPost("Add-New-Showtime")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateNewShowtime([FromBody] CreateShowtimeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var movieExists = await _unitOfWork.Movies.AnyAsync(m => m.Id == dto.MovieId);
            var hallExists = await _unitOfWork.Halls.AnyAsync(h => h.Id == dto.HallId);

            if (!movieExists)
                return NotFound($"Movie with Id {dto.MovieId} not found.");
            if (!hallExists)
                return NotFound($"Hall with Id {dto.HallId} not found.");

            var showtime = new Showtime
            {
                MovieId = dto.MovieId,
                HallId = dto.HallId,
                StartsAt = dto.StartsAt,
                Price = dto.Price
            };

            _unitOfWork.Showtimes.Add(showtime);
            _unitOfWork.Complete();

            return CreatedAtAction(nameof(FindById), new { id = showtime.Id }, showtime);
        }

        // ====== UPDATE ====== //

        [HttpPut("Update-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateShowtime(int id, [FromBody] UpdateShowtimeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var showtime = await _unitOfWork.Showtimes.FindAsync(i => i.Id == id);
            if (showtime == null)
                return NotFound($"Showtime with Id {id} not found.");

            var movieExists = await _unitOfWork.Movies.AnyAsync(m => m.Id == dto.MovieId);
            var hallExists = await _unitOfWork.Halls.AnyAsync(h => h.Id == dto.HallId);

            if (!movieExists)
                return NotFound($"Movie with Id {dto.MovieId} not found.");
            if (!hallExists)
                return NotFound($"Hall with Id {dto.HallId} not found.");

            showtime.MovieId = dto.MovieId;
            showtime.HallId = dto.HallId;
            showtime.StartsAt = dto.StartsAt;
            showtime.Price = dto.Price;

            _unitOfWork.Complete();

            return NoContent();
        }

        // ====== DELETE ====== //

        [HttpDelete("Delete-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(int id)
        {
            var showtime = await _unitOfWork.Showtimes.FindAsync(h => h.Id == id);
            if (showtime == null)
                return NotFound(new { message = "No showtime found with this Id" });

            var deleted = await _unitOfWork.Showtimes.DeleteAsync(showtime.Id);
            _unitOfWork.Complete();

            return Ok(new { message = $"This showtime has been successfully deleted." });
        }
    }
}
