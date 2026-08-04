using MakanPergi.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakanPergi.Entities
{
    [Table("Food")]
    [Index(nameof(Name), IsUnique = true)]
    public class Food : Entity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public string? Image { get; set; }
        public Cuisine? Cuisine { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Shop> ShopsOffered { get; set; }

    }
}
