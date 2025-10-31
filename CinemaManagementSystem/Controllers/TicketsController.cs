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
    public class TicketsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TicketsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ====== READ ====== //

        [HttpGet("All-Tickets")]
        [ProducesResponseType(typeof(IEnumerable<Ticket>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTickets([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            if (take <= 0) take = 50;
            if (take > 200) take = 200;

            var qTickets = _unitOfWork.Tickets.QueryNoTracking();
            var qShow = _unitOfWork.Showtimes.QueryNoTracking();
            var qMovies = _unitOfWork.Movies.QueryNoTracking();
            var qHalls = _unitOfWork.Halls.QueryNoTracking();
            var qSeats = _unitOfWork.Seats.QueryNoTracking();
            var qCusts = _unitOfWork.Customers.QueryNoTracking();

            var data =
                await (from t in qTickets

                        // LEFT JOIN 
                       join st in qShow on t.ShowtimeId equals st.Id into stg
                       from st in stg.DefaultIfEmpty()

                       join mv in qMovies on st.MovieId equals mv.Id into mvg
                       from mv in mvg.DefaultIfEmpty()

                       join hl in qHalls on st.HallId equals hl.Id into hlg
                       from hl in hlg.DefaultIfEmpty()

                       join se in qSeats on t.SeatId equals se.Id into seg
                       from se in seg.DefaultIfEmpty()

                       join cu in qCusts on t.CustomerId equals cu.Id into cug
                       from cu in cug.DefaultIfEmpty()

                       orderby t.Id
                       select new ReadTicketDTO
                       {
                           TicketId = t.Id,
                           MovieTitle = mv != null ? mv.Title : "(No movie)",
                           HallName = hl != null ? hl.Name : "(No hall)",
                           SeatLabel = se != null ? (se.Row + "-" + se.Number) : "(No seat)",
                           CustomerName = cu != null ? cu.FullName : "(No customer)",
                           PurchasedAtUtc = t.PurchasedAt
                       })
                      .Skip(skip).Take(take)
                      .ToListAsync();

            return Ok(data);
        }

        [HttpGet("Find-By-Id/{id:int}")]
        [ProducesResponseType(typeof(Seat), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ticket>> FindById(int id)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);

            if (ticket == null)
                return NotFound();

            return ticket;
        }

        // ====== CREATE ====== //

        [HttpPost("Add-New-Ticket")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReadTicketDTO>> CreateNewTicket([FromBody] CreateTicketDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool seatTaken = await _unitOfWork.Tickets.AnyAsync(t =>
                t.SeatId == dto.SeatId && t.ShowtimeId == dto.ShowtimeId);

            if (seatTaken)
                return Conflict("This seat is already reserved for this show!");

            var ticket = new Ticket
            {
                ShowtimeId = dto.ShowtimeId,
                SeatId = dto.SeatId,
                CustomerId = dto.CustomerId,
                PurchasedAt = dto.PurchasedAt
            };

            _unitOfWork.Tickets.Add(ticket);
            _unitOfWork.Complete();

            return CreatedAtAction(nameof(FindById), new { id = ticket.Id }, ticket);
        }

        // ====== UPDATE ====== //

        [HttpPut("Update-By-Id/{id:int}")]
        public async Task<IActionResult> UpdateById(int id, [FromBody] UpdateTicketDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = await _unitOfWork.Tickets.FindAsync(i => i.Id == id);
            if (ticket == null)
                return NotFound("The ticket is not exist.");

            // 🔹 التحقق من أن المقعد الجديد غير محجوز لنفس العرض (باستثناء نفسه)
            bool seatTaken = await _unitOfWork.Tickets.AnyAsync(t =>
                t.Id != id && t.SeatId == dto.SeatId && t.ShowtimeId == dto.ShowtimeId);

            if (seatTaken)
                return Conflict("This seat is already reserved for this show!");

            // 🔹 تحديث القيم
            ticket.ShowtimeId = dto.ShowtimeId;
            ticket.SeatId = dto.SeatId;
            ticket.CustomerId = dto.CustomerId;
            ticket.PurchasedAt = dto.PurchasedAt;

            _unitOfWork.Complete();

            return NoContent();
        }

        // ====== DELETE ====== //

        [HttpDelete("Delete-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(int id)
        {
            var ticket = await _unitOfWork.Tickets.FindAsync(h => h.Id == id);
            if (ticket == null)
                return NotFound(new { message = "No ticket found with this Id" });

            var deleted = await _unitOfWork.Tickets.DeleteAsync(ticket.Id);
            _unitOfWork.Complete();

            return Ok(new { message = $"This ticket has been successfully deleted." });
        }  

    }
}

