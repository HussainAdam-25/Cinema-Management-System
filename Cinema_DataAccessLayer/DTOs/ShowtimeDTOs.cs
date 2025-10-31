using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_DataAccessLayer.DTOs
{
    public class CreateShowtimeDto
    {
        [Required]
        public int MovieId { get; set; }

        [Required]
        public int HallId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Starts At")]
        public DateTime StartsAt { get; set; }

        [Required]
        [Range(0.0, 200.0, ErrorMessage = "Price must be between 0 and 200.")]
        public decimal Price { get; set; }
    }

    public class UpdateShowtimeDto
    {
        [Required]
        public int MovieId { get; set; }

        [Required]
        public int HallId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartsAt { get; set; }

        [Required]
        [Range(0.0, 200.0)]
        public decimal Price { get; set; }
    }

    public class ReadShowtimeDto
    {
        public int ShowtimeId { get; set; }
        public string MovieTitle { get; set; }  
        public string HallName { get; set; }    
        public DateTime StartsAtUtc { get; set; }
        public decimal Price { get; set; }
    }
}



