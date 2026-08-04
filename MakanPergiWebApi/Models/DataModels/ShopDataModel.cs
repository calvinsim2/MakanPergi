namespace MakanPergiWebApi.Models.DataModels
{
    public class ShopDataModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Guid> Foods { get; set; } = new List<Guid>();
        public string? Image { get; set; }
        public string? Address { get; set; }
        public Guid? RegionId { get; set; }
    }
}
