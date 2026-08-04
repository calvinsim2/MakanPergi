using MakanPergi.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakanPergi.Entities
{
    [Table("Cuisine")]
    [Index(nameof(Name), IsUnique = true)]
    public class Cuisine : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
