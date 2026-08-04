namespace MakanPergiWebApi.Models.ViewModels
{
    public class FoodRelationViewModel
    {
        public string Name { get; set; }
        public string? Image { get; set; }
        public CuisineViewModel? Cuisine { get; set; }
    }
}
