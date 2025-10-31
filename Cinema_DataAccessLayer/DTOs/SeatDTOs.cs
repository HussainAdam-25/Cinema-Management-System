using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_DataAccessLayer.DTOs
{
    public class CreateSeatDto
    {
        [Required]
        public int HallId { get; set; }

        [Required]
        [StringLength(10)]
        public string Row { get; set; } = string.Empty;

        [Required]
        [Range(1, 250, ErrorMessage = "Seat number must be between 1 and 250.")]
        public int Number { get; set; }
    }

    public class UpdateSeatDto
    {
        [Required]
        public int HallId { get; set; }

        [Required]
        [StringLength(10)]
        public string Row { get; set; } = string.Empty;

        [Required]
        [Range(1, 250)]
        public int Number { get; set; }
    }

    public class ReadSeatDTO
    {
        public int Id { get; set; }
        public string Row { get; set; }
        public int Number { get; set; }
        public string HallName { get; set; }
    }


}
