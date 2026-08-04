using AutoMapper;
using Common.Model;
using MakanPergi.Entities;
using MakanPergiWebApi.Models.DataModels;
using MakanPergiWebApi.Models.Dtos.Category;
using MakanPergiWebApi.Models.Dtos.Cuisine;
using MakanPergiWebApi.Models.Dtos.Food;
using MakanPergiWebApi.Models.Dtos.Shop;
using MakanPergiWebApi.Models.ViewModels;

namespace MakanPergiWebApi.Helpers
{
    public class GlobalProfile : Profile
    {
        public GlobalProfile() 
        {
            CreateMap<CuisineViewModel, CuisineDataModel>().ReverseMap();
            CreateMap<CuisineDataModel, Cuisine>().ReverseMap();
            CreateMap<CreateCuisineDto, CuisineDataModel>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<EditCuisineDto, CuisineDataModel>();
            CreateMap<EditCuisineDto, CuisineViewModel>().ReverseMap();
            CreateMap<CuisineViewModel, EditCuisineDto>().ReverseMap();

            CreateMap<CategoryViewModel, CategoryDataModel>()
                        .ForMember(dest => dest.Foods, opt => opt.Ignore())
                        .ReverseMap();
            CreateMap<CategoryDataModel, Category>()
                        .ForMember(dest => dest.Foods, opt => opt.Ignore())
                        .ReverseMap();
            CreateMap<CreateCategoryDto, CategoryDataModel>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.Foods, opt => opt.Ignore());
            CreateMap<EditCategoryDto, CategoryDataModel>()
                        .ForMember(dest => dest.Foods, opt => opt.Ignore());
            CreateMap<CreateShopDto, ShopDataModel>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.Foods, opt => opt.Ignore());
            CreateMap<EditShopDto, ShopDataModel>()
                        .ForMember(dest => dest.Foods, opt => opt.Ignore());

            CreateMap<CreateFoodDto, FoodDataModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, 
                                   opt => opt.MapFrom(src => src.CategoryNames.Select(cat => cat.Id).ToList()))
                .ForMember(dest => dest.ShopsOffered, opt => opt.MapFrom(src => src.ShopNames.Select(shop => shop.Id).ToList()));

            CreateMap<EditFoodDto, FoodDataModel>()
                .ForMember(dest => dest.Categories,
                                   opt => opt.MapFrom(src => src.CategoryNames.Select(cat => cat.Id).ToList()))
                .ForMember(dest => dest.ShopsOffered, opt => opt.MapFrom(src => src.ShopNames.Select(shop => shop.Id).ToList()));

            CreateMap<FoodEditViewModel, FoodDataModel>()
                .ForMember(dest => dest.Categories,
                                   opt => opt.MapFrom(src => src.CategoryNames.Select(cat => cat.Id).ToList()))
                .ForMember(dest => dest.ShopsOffered, opt => opt.MapFrom(src => src.ShopNames.Select(shop => shop.Id).ToList()));

            CreateMap<Food, FoodViewModel>();
            CreateMap<Food, FoodRelationViewModel>();
            CreateMap<Category, CategoryRelationViewModel>();
            CreateMap<Shop, ShopRelationViewModel>();
            CreateMap<Cuisine, CuisineViewModel>();

            CreateMap<Category, TreeViewNodeModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom( src => src.Id))
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Text, opt => opt.MapFrom( src => src.Name))
                .ForPath( dest => dest.State.Disabled, opt => opt.MapFrom(_ => false))
                .ReverseMap();

            CreateMap<Shop, TreeViewNodeModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Parent, opt => opt.MapFrom(src => src.RegionId == null ? "#" : src.RegionId.ToString()))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name))
                .ForPath( dest => dest.State.Disabled, opt => opt.MapFrom(_ => false))
                .ReverseMap();
        }
    }
}
