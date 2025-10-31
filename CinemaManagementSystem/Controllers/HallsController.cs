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
    public class HallsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public HallsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ====== READ ====== //

        [HttpGet("All-Halls")]
        [ProducesResponseType(typeof(IEnumerable<Hall>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllHalls()
        {
            var items = await _unitOfWork.Halls.GetAllAsync();

            return Ok(items);
        }

        [HttpGet("Find-By-Id/{id:int}")]
        [ProducesResponseType(typeof(Hall), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Hall>> FindById(int id)
        {
            var hall = await _unitOfWork.Halls.GetByIdAsync(id);
            if (hall == null)
                return NotFound();
            return hall;
        }

        [HttpGet("Find-By-Name")]
        [ProducesResponseType(typeof(IEnumerable<Hall>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Hall>> FindByName(string name)
        {
            var hall = await _unitOfWork.Halls.FindAsync(h => h.Name == name);
            if (hall == null)
                return NotFound();
            return hall;
        }

        // ====== CREATE ====== //

        [HttpPost("Add-New-Hall")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Hall>> CreateNewHall([FromBody] CreateHallDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid hall data.");

            var name = dto.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Hall name is required.");

            // التحقق من التكرار
            var exists = await _unitOfWork.Halls.AnyAsync(h => h.Name.ToLower() == name.ToLower());
            if (exists)
                return Conflict("A hall with this name already exists.");

            var entity = new Hall
            {
                Name = name,
                Capacity = dto.Capacity
            };

            var created = await _unitOfWork.Halls.AddAsync(entity);

            var id = created.Id; _unitOfWork.Complete();

            return CreatedAtAction(nameof(FindById), new { id }, created);
        }

        // ====== UPDATE ====== //

        [HttpPut("Update-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateById(int id, [FromBody] UpdateHallDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid hall data.");

            var hall = await _unitOfWork.Halls.GetByIdAsync(id);
            if (hall == null)
                return NotFound("Hall not found.");

            var name = dto.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Hall name is required.");

            // تحقق من وجود اسم مكرر في سجل آخر
            var exists = await _unitOfWork.Halls.AnyAsync(h => h.Id != id && h.Name.ToLower() == name.ToLower());
            if (exists)
                return Conflict("Another hall with this name already exists.");

            hall.Name = name;
            hall.Capacity = dto.Capacity ?? hall.Capacity;

            _unitOfWork.Halls.Update(hall);
            _unitOfWork.Complete();

            return NoContent();
        }

        [HttpPut("Update-By-Name")]
        public async Task<IActionResult> UpdateByName([FromQuery] string name, [FromBody] UpdateHallDto dto)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Hall name is required.");

            // البحث عن القاعة بالاسم
            var hall = await _unitOfWork.Halls
                .Query()
                .FirstOrDefaultAsync(h => h.Name.ToLower() == name.ToLower());

            if (hall == null)
                return NotFound($"No hall found with the name '{name}'.");

            // تحديث القيم
            hall.Name = dto.Name;
            hall.Capacity = dto.Capacity ?? hall.Capacity;

            _unitOfWork.Halls.Update(hall);
            _unitOfWork.Complete();

            return NoContent();
        }

        // ====== DELETE ====== //

        [HttpDelete("Delete-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(int id)
        {
            var hall = await _unitOfWork.Halls.FindAsync(h => h.Id == id);
            if (hall == null)
                return NotFound(new { message = "No hall found with this Id" });

            var deleted = await _unitOfWork.Halls.DeleteAsync(hall.Id);
            _unitOfWork.Complete();

            return Ok(new { message = $"Hall has been successfully deleted." });
        }

        [HttpDelete("Delete-By-Name")]
        public async Task<IActionResult> DeleteByName(string name)
        {
            var hall = await _unitOfWork.Halls.FindAsync(h => h.Name == name);
            if (hall == null)
                return NotFound(new { message = "No hall found with this name" });

            var deleted = await _unitOfWork.Halls.DeleteAsync(hall.Id);
            _unitOfWork.Complete();

            return Ok(new { message = $"Hall '{name}' deleted successfully" });
        }
    }
}
