using AutoMapper;
using Common.Model;
using MakanPergi.Models.Category;
using MakanPergi.Models.Cuisine;
using MakanPergi.Models.Food;
using MakanPergi.Models.Shop;

namespace MakanPergi.Common.Helpers
{
    public class GlobalProfile : Profile
    {
        public GlobalProfile()
        {
            CreateMap<EditCuisineDto, CuisineViewModel>().ReverseMap();
            CreateMap<CuisineViewModel, EditCuisineDto>().ReverseMap();

        }
    }
}
