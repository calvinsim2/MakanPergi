using MakanPergi.Models.Cuisine;

namespace MakanPergi.Models.Food
{
    public class FoodRelationViewModel
    {
        public string Name { get; set; }
        public string? Image { get; set; }
        public CuisineViewModel? Cuisine { get; set; }
    }
}
