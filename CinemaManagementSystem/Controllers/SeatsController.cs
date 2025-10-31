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
    public class SeatsController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        public SeatsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ====== READ ====== //

        [HttpGet("All-Seats")]
        [ProducesResponseType(typeof(IEnumerable<Seat>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllSeats()
        {
            var seats = await _unitOfWork.Seats.Query()
            .Select(s => new ReadSeatDTO
            {
               Id = s.Id,
               Row = s.Row,
               Number = s.Number,
               HallName = s.Hall.Name      
            })
            .ToListAsync();

            return Ok(seats);
        }

        [HttpGet("Find-By-Id/{id:int}")]
        [ProducesResponseType(typeof(Seat), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Seat>> FindById(int id)
        {
            var seat = await _unitOfWork.Seats.GetByIdAsync(id);
            if (seat == null)
                return NotFound();
            return seat;
        }

        // ====== CREATE ====== //

        [HttpPost("Add-New-Seat")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CreateSeatDto>> CreateNewSeat([FromBody] CreateSeatDto input)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var normalizedRow = (input.Row ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalizedRow))
                return BadRequest(new { message = "Row cannot be empty!" });

            var exists = await _unitOfWork.Seats
                .QueryNoTracking()
                .AnyAsync(s => s.HallId == input.HallId &&
                               s.Row == normalizedRow &&
                               s.Number == input.Number);
            if (exists)
                return Conflict(new
                {
                    code = "SeatExists",
                    message = "This seat is existing before in this hall."
                });

            var entity = new Seat
            {
                HallId = input.HallId,
                Row = normalizedRow,
                Number = input.Number
            };

            var created = await _unitOfWork.Seats.AddAsync(entity);

            var id = created.Id;

            _unitOfWork.Complete();

            return CreatedAtAction(nameof(FindById), new { id }, created);
        }

        // ====== UPDATE ====== //

        [HttpPut("Update-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateSeat(int id, [FromBody] UpdateSeatDto input)
        {
            // 1) تحقق من صحة الداتا
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2) احضر المقعد المطلوب تحديثه
            var seat = await _unitOfWork.Seats.FindAsync(i => i.Id == id); 
            if (seat is null)
                return NotFound();

            var duplicate = await _unitOfWork.Seats.AnyAsync(s =>
                s.Id != id &&
                s.HallId == input.HallId &&
                s.Row == input.Row &&
                s.Number == input.Number);

            if (duplicate)
                return Conflict("Seat with the same Hall/Row/Number already exists.");

            // 4) عدّل الحقول من الـ DTO
            seat.HallId = input.HallId;
            seat.Row = input.Row?.Trim();
            seat.Number = input.Number;

            // 5) احفظ التغييرات مع معالجة تعارضات التزامن
            try
            {
                _unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _unitOfWork.Seats.AnyAsync(s => s.Id == id);
                if (!exists) return NotFound();
                throw;
            }

            return NoContent();
        }

        // ====== DELETE ====== //

        [HttpDelete("Delete-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(int id)
        {
            var seat = await _unitOfWork.Seats.FindAsync(h => h.Id == id);
            if (seat == null)
                return NotFound(new { message = "No seat found with this Id" });

            var deleted = await _unitOfWork.Seats.DeleteAsync(seat.Id);
            _unitOfWork.Complete();

            return Ok(new { message = $"This seat has been successfully deleted." });
        }
    }
}
