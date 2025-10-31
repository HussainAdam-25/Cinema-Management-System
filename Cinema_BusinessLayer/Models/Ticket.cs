using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_BusinessLayer.Models
{
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ShowtimeId { get; set; }

        [Required]
        public int SeatId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime PurchasedAt { get; set; }


        [ForeignKey("ShowtimeId")]
        public Showtime Showtime { get; set; } = default!;

        [ForeignKey("SeatId")]
        public Seat Seat { get; set; } = default!;

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = default!;
    }
}
