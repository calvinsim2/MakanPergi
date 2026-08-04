using Common.TagHelper;

namespace MakanPergi.Models.Food
{
    public class FoodFullViewModel
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
