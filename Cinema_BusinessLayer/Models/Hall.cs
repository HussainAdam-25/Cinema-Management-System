using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_BusinessLayer.Models
{
    [Table("Halls")]
    public class Hall
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; }

        [Required]
        [StringLength(50)] // nvarchar(50)
        public string Name { get; set; } = default!;

        [Required]
        public int Capacity { get; set; }

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }

}
