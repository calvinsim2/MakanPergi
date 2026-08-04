namespace MakanPergiWebApi.Models.ViewModels
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<FoodRelationViewModel> Foods { get; set; } = new List<FoodRelationViewModel>();
    }
}
