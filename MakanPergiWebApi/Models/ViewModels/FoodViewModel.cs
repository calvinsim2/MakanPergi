namespace MakanPergiWebApi.Models.ViewModels
{
    public class FoodViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public CuisineViewModel? Cuisine { get; set; }
        public List<CategoryRelationViewModel> Categories { get; set; }
        public List<ShopRelationViewModel> ShopsOffered { get; set; }
    }
}

