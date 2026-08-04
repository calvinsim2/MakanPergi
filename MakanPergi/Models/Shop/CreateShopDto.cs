namespace MakanPergi.Models.Shop
{
    public class CreateShopDto
    {
        public string Name { get; set; }
        public string? Image { get; set; }
        public string Address { get; set; }
        public Guid? RegionId { get; set; }
    }
}
