using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_BusinessLayer.Models
{
    [Table("Showtimes")]
    public class Showtime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int HallId { get; set; }

        [Required]
        public DateTime StartsAt { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [ForeignKey("MovieId")]
        public Movie Movie { get; set; } = default!;

        [ForeignKey("HallId")]
        public Hall Hall { get; set; } = default!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
