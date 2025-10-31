using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_BusinessLayer.Models
{
    [Table("Movies")] 
    public class Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = default!;

        [Required, MaxLength(50)]
        public string? Genre { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int DurationMinutes { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime ReleaseDate { get; set; }           
    }
}
