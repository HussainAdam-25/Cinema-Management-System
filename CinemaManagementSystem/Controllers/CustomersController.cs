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
    public class CustomersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ====== READ ====== //

        [HttpGet("All-Customers")]
        [ProducesResponseType(typeof(IEnumerable<Customer>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCustomers()
        {
            var items = await _unitOfWork.Customers.GetAllAsync();

            return Ok(items);
        }

        [HttpGet("Find-By-Id/{id:int}")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindById([FromRoute] int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return customer is null ? NotFound() : Ok(customer);
        }

        [HttpGet("Find-By-Name")]
        [ProducesResponseType(typeof(IEnumerable<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindByName([FromQuery] string name)
        {
            var result = await _unitOfWork.Customers.FindAsync(c => c.FullName == name);
            return Ok(result);
        }

        [HttpGet("Find-By-PhoneNumber")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindByPhoneNumber([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest("phone is required.");

            var normalized = NormalizePhone(phone);

            var customer = await _unitOfWork.Customers.FindAsync(
                c => c.Phone == normalized
            );

            return customer is null ? NotFound() : Ok(customer);
        }

        [HttpGet("Find-By-Email")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("email is required.");

            var q = email.Trim().ToLowerInvariant();

            var customer = await _unitOfWork.Customers.FindAsync(
                c => c.Email.ToLower() == q
            );

            return customer is null ? NotFound() : Ok(customer);
        }

        // ====== CREATE ====== //

        [HttpPost("Add-New-Customer")]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : NormalizePhone(dto.Phone);
            var email = string.IsNullOrWhiteSpace(dto.Email) ? null : NormalizeEmail(dto.Email);

            if (!string.IsNullOrEmpty(phone))
            {
                bool existsPhone = await _unitOfWork.Customers
                    .QueryNoTracking()
                    .AnyAsync(c => c.Phone != null && c.Phone == phone);
                if (existsPhone)
                    return Conflict("Phone is already used.");
                if (!string.IsNullOrEmpty(phone) && phone.Length > 15)
                    return BadRequest("Invalid phone format.");
            }

            if (!string.IsNullOrEmpty(email))
            {
                bool existsEmail = await _unitOfWork.Customers
                    .QueryNoTracking()
                    .AnyAsync(c => c.Email != null && c.Email.Trim().ToLower() == email);
                if (existsEmail)
                    return Conflict("Email is already used.");
            }

            var entity = new Customer
            {
                FullName = dto.FullName.Trim(),
                Phone = phone,
                Email = email
            };

            var created = await _unitOfWork.Customers.AddAsync(entity);

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
        public async Task<IActionResult> UpdateById([FromRoute] int id, [FromBody] CustomerUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _unitOfWork.Customers.GetByIdAsync(id);
            if (existing is null) return NotFound();

            var phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : NormalizePhone(dto.Phone);
            var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();

            // لو تغيّر الهاتف/الإيميل، تأكّد من عدم تعارضه مع زبون آخر
            if (!string.IsNullOrEmpty(phone) && phone != existing.Phone)
            {
                var other = await _unitOfWork.Customers.FindAsync(c => c.Phone == phone);
                if (other is not null && other.Id != id) return Conflict("Phone is already used.");
            }
            if (!string.IsNullOrEmpty(email) && email != (existing.Email?.ToLower()))
            {
                var other = await _unitOfWork.Customers.FindAsync(c => c.Email.ToLower() == email);
                if (other is not null && other.Id != id) return Conflict("Email is already used.");
            }

            existing.FullName = dto.FullName?.Trim() ?? existing.FullName;
            existing.Phone = phone;
            existing.Email = email;

            await _unitOfWork.Customers.UpdateAsync(existing);
            _unitOfWork.Complete();

            return NoContent();
        }

        [HttpPut("Update-By-Email")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateByEmail([FromQuery] string email, [FromBody] CustomerUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("email is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var query = email.Trim().ToLowerInvariant();

            // ابحث عن العميل الحالي بهذا الإيميل
            var existing = await _unitOfWork.Customers.FindAsync(c => c.Email.ToLower() == query);
            if (existing is null) return NotFound();

            // حضّر القيم الجديدة (اختيارية)
            string? newPhone = null;
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                try { newPhone = NormalizePhone(dto.Phone); }
                catch (ArgumentException ex) { return BadRequest(ex.Message); }
            }

            string? newEmail = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();

            // تحقق من التعارض مع عملاء آخرين
            if (!string.IsNullOrEmpty(newPhone) && newPhone != existing.Phone)
            {
                var other = await _unitOfWork.Customers.FindAsync(c => c.Phone == newPhone);
                if (other is not null && other.Id != existing.Id)
                    return Conflict("Phone is already used.");
            }
            if (!string.IsNullOrEmpty(newEmail) && newEmail != (existing.Email?.ToLower()))
            {
                var other = await _unitOfWork.Customers.FindAsync(c => c.Email.ToLower() == newEmail);
                if (other is not null && other.Id != existing.Id)
                    return Conflict("Email is already used.");
            }

            // حدّث الحقول المطلوبة فقط
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                existing.FullName = dto.FullName.Trim();
            if (dto.Phone != null)  // السماح بالحذف لو null
                existing.Phone = newPhone;
            if (dto.Email != null)
                existing.Email = newEmail;

            await _unitOfWork.Customers.UpdateAsync(existing);
            _unitOfWork.Complete();

            return NoContent();
        }

        [HttpPut("Update-By-PhoneNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateByPhone([FromQuery] string phone, [FromBody] CustomerUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(phone)) return BadRequest("phone is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string normalized;
            try { normalized = NormalizePhone(phone); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }

            // ابحث عن العميل الحالي بهذا الهاتف
            var existing = await _unitOfWork.Customers.FindAsync(c => c.Phone == normalized);
            if (existing is null) return NotFound();

            // حضّر القيم الجديدة (اختيارية)
            string? newPhone = null;
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                try { newPhone = NormalizePhone(dto.Phone); }
                catch (ArgumentException ex) { return BadRequest(ex.Message); }
            }

            string? newEmail = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();

            // تحقق من التعارض مع عملاء آخرين
            if (!string.IsNullOrEmpty(newPhone) && newPhone != existing.Phone)
            {
                var other = await _unitOfWork.Customers.FindAsync(c => c.Phone == newPhone);
                if (other is not null && other.Id != existing.Id)
                    return Conflict("Phone is already used.");
            }
            if (!string.IsNullOrEmpty(newEmail) && newEmail != (existing.Email?.ToLower()))
            {
                var other = await _unitOfWork.Customers.FindAsync(c => c.Email.ToLower() == newEmail);
                if (other is not null && other.Id != existing.Id)
                    return Conflict("Email is already used.");
            }

            // التحديث
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                existing.FullName = dto.FullName.Trim();
            if (dto.Phone != null)
                existing.Phone = newPhone;
            if (dto.Email != null)
                existing.Email = newEmail;

            await _unitOfWork.Customers.UpdateAsync(existing);
            _unitOfWork.Complete();

            return NoContent();
        }

        // ====== DELETE ====== //

        [HttpDelete("Delete-By-Id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById([FromRoute] int id)
        {
            var ok = await _unitOfWork.Customers.DeleteAsync(id);
            _unitOfWork.Complete();

            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("Delete-By-Email")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("email is required.");

            var query = email.Trim().ToLowerInvariant();

            // ابحث عن العميل بالبريد الإلكتروني
            var customer = await _unitOfWork.Customers.FindAsync(c => c.Email.ToLower() == query);
            if (customer is null)
                return NotFound("No customer found with this email.");

            // احذف باستخدام المعرف
            var deleted = await _unitOfWork.Customers.DeleteAsync(customer.Id);
            _unitOfWork.Complete();

            return deleted ? NoContent() : NotFound();
        }

        [HttpDelete("Delete-By-PhoneNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByPhoneNumber([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest("phone is required.");

            string normalized;
            try
            {
                normalized = NormalizePhone(phone); // توحيد صيغة الرقم
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            // ابحث عن العميل برقم الهاتف
            var customer = await _unitOfWork.Customers.FindAsync(c => c.Phone == normalized);
            if (customer is null)
                return NotFound("No customer found with this phone.");

            // احذف باستخدام المعرف
            var deleted = await _unitOfWork.Customers.DeleteAsync(customer.Id);
            _unitOfWork.Complete();

            return deleted ? NoContent() : NotFound();
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("971"))
                return "+971" + digits.Substring(3);

            if (digits.StartsWith("0") && digits.Length == 10)
                return "+971" + digits.Substring(1);

            if (digits.Length == 9)
                return "+971" + digits;

            return "+" + digits; // fallback
        }

        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            return email.Trim().ToLowerInvariant();
        }

    }
}
