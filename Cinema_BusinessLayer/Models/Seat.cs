using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_BusinessLayer.Models
{
    [Table("Seats")]
    public class Seat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Row { get; set; } = null!;

        [Required]
        public int Number { get; set; }

        [Required]
        public int HallId { get; set; }

        [ForeignKey("HallId")]
        public Hall Hall { get; set; } = default!;
    }
}
