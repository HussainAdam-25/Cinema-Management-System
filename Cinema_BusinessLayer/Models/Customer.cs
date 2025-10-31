using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema_BusinessLayer.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string FullName { get; set; } = default!;

        [StringLength(120)] 
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(15)] 
        public string? Phone { get; set; }
    }
}
