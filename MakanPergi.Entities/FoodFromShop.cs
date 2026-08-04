using MakanPergi.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakanPergi.Entities
{
    [Table("FoodFromShop")]
    public class FoodFromShop : AbstractBaseEntity
    {
        public Guid FoodId { get; set; }
        public Guid ShopId { get; set; }
    }
}
