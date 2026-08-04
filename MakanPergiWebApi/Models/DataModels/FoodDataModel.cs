namespace MakanPergiWebApi.Models.DataModels
{
    public class FoodDataModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public Guid? CuisineId { get; set; }
        public virtual ICollection<Guid> Categories { get; set; } = new List<Guid>();
        public virtual ICollection<Guid> ShopsOffered { get; set; } = new List<Guid>();
    }
}
