namespace MakanPergiWebApi.Models.DataModels
{
    public class CategoryDataModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Guid>? Foods { get; set; } = new List<Guid>();
    }
}
