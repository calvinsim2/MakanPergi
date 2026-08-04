using MakanPergi.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakanPergi.Entities
{
    [Table("FoodCategory")]
    public class FoodCategory : AbstractBaseEntity
    {
        public Guid FoodId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
