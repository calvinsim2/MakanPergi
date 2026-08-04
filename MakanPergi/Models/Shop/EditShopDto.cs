namespace MakanPergi.Models.Shop
{
    public class EditShopDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public string Address { get; set; }
        public Guid? RegionId { get; set; }
    }
}
