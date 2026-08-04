using MakanPergi.Models.Food;

namespace MakanPergi.Models.Shop
{
    public class ShopViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<FoodRelationViewModel> Foods { get; set; } = new List<FoodRelationViewModel>();
        public string? Image { get; set; }
        public string? Address { get; set; }
        public Guid? RegionId { get; set; }
    }
}
