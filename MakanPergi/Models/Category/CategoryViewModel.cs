using MakanPergi.Models.Food;

namespace MakanPergi.Models.Category
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<FoodRelationViewModel> Foods { get; set; }
    }
}
