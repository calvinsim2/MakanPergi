using Common.TagHelper;
using MakanPergi.Entities.Base;

namespace MakanPergi.Entities.Views
{
    public class VW_FoodFullView : AbstractBaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        [DisplayWithSpaces]
        public string? CuisineName { get; set; }
        [DisplayWithSpaces]
        public string? CategoryNames { get; set; }
        [DisplayWithSpaces]
        public string? ShopNames { get; set; }
    }
}
