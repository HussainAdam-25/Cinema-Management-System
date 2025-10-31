using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cinema_DataAccessLayer.DTOs
{
    public class CustomerCreateDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }

    public class CustomerUpdateDto
    {
        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}
