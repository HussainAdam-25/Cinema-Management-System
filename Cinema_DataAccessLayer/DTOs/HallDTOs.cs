using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_DataAccessLayer.DTOs
{
    public class CreateHallDto
    {
        [Required(ErrorMessage = "Hall name is required.")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Hall name must be 2–10 characters.")]
        [RegularExpression(@"^(?=.*\S).+$", ErrorMessage = "Hall name cannot be empty or whitespace.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 250, ErrorMessage = "Capacity must be between 1 and 250.")]
        public int Capacity { get; set; }
    }

    public class UpdateHallDto
    {
        [StringLength(10, MinimumLength = 2)]
        [RegularExpression(@"^(?=.*\S).+$")]
        public string? Name { get; set; }

        [Range(1, 250)]
        public int? Capacity { get; set; }
    }

}
