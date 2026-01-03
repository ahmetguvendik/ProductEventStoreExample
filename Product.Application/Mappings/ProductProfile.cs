using AutoMapper;
using ProductViewModel = Product.Application.Models.ProductViewModel;
using ProductUpdateViewModel = Product.Application.Models.ProductUpdateViewModel;
using ProductCreateViewModel = Product.Application.Models.ProductCreateViewModel;
using ProductEntity = Product.Application.Models.Product;

namespace Product.Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductEntity, ProductViewModel>().ReverseMap();
        CreateMap<ProductEntity, ProductUpdateViewModel>().ReverseMap();
        CreateMap<ProductCreateViewModel, ProductEntity>();
    }
}

