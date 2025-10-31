using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_DataAccessLayer.DTOs
{
    public class CreateMovieDto
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Movie Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Genre")]
        public string? Genre { get; set; }

        [Required]
        [Range(1, 210, ErrorMessage = "Duration must be between 1 and 210 minutes.")]
        [Display(Name = "Duration (Minutes)")]
        public int DurationMinutes { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }
    }

    public class UpdateMovieDto
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Genre { get; set; }

        [Required]
        [Range(1, 210)]
        public int DurationMinutes { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ReleaseDate { get; set; }
    }

    public record MovieReadDto(
    int Id,
    string Title,
    string? Genre,
    int DurationMinutes,
    DateTime ReleaseDate);

}
