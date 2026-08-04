using Common.Model;
using Common.ModelBinders;
using Microsoft.AspNetCore.Mvc;

namespace MakanPergi.Models.Food
{
    public class FoodEditViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public Guid CuisineId { get; set; }

        [ModelBinder(BinderType = typeof(TreeViewNodeModelCustomBinder))]
        public TreeViewNodeModelDTO[] CategoryNames { get; set; }

        [ModelBinder(BinderType = typeof(TreeViewNodeModelCustomBinder))]
        public TreeViewNodeModelDTO[] ShopNames { get; set; }
    }
}
