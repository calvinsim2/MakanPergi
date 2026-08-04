using Common.TagHelper;

namespace MakanPergi.Models.Shop
{
    public class ShopIndexViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        [DisplayWithSpaces]
        public string? RegionName { get; set; }
    }
}
