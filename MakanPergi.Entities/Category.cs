using Common.Constants;
using MakanPergi.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakanPergi.Entities
{
    [Table("Category")]
    [Index(nameof(Name), IsUnique = true)]
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = DefaultValue.Undefined;
        public virtual ICollection<Food> Foods { get; set; }
    }
}
