using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_DataAccessLayer.DTOs
{
    public class CreateTicketDto
    {
        [Required]
        public int ShowtimeId { get; set; }

        [Required]
        public int SeatId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Purchased At")]
        public DateTime PurchasedAt { get; set; }
    }

    public class UpdateTicketDto
    {
        [Required]
        public int ShowtimeId { get; set; }

        [Required]
        public int SeatId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime PurchasedAt { get; set; }
    }

    public class ReadTicketDTO
    {
        public int TicketId { get; set; }
        public string MovieTitle { get; set; }
        public string HallName { get; set; }
        public string SeatLabel { get; set; }
        public string CustomerName { get; set; }
        public DateTime PurchasedAtUtc { get; set; }
    }


}
