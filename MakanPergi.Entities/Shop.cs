using MakanPergi.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakanPergi.Entities
{
    [Table("Shop")]
    [Index(nameof(Name), IsUnique = true)]
    public class Shop : Entity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Food> Foods { get; set; }

        public string? Image { get; set; }
        [Required]
        [StringLength(500)]
        public string? Address { get; set; }
        [ForeignKey("Region")]
        public Guid? RegionId { get; set; }
        [InverseProperty("RegionShops")]
        public Shop Region { get; set; }
        [ForeignKey("RegionId")]
        public ICollection<Shop> RegionShops { get; set; }
    }
}
