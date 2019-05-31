using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Intertek.DataModels.Outputmodels;


namespace SG.PO.Intertek.CommandService.Core.Mapper
{
    public class POProductProfile : AutoMapper.Profile
    {
        public POProductProfile()
        {
            CreateMap<POProduct, POProductOutput>()
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class))
                .ForMember(dest => dest.ClassShortDesc, opt => opt.MapFrom(src => src.ClassShortDesc))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.ClassDescription))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassDescription))
                .ForMember(dest => dest.DepartmentCode, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.HTSCode, opt => opt.MapFrom(src => src.HTSCode))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.CountryOfOrigin))
                .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.Sku))
                .ForMember(dest => dest.MasterPackQuantity, opt => opt.MapFrom(src => src.MasterPackQuantity))
                .ForMember(dest => dest.SkuDesc, opt => opt.MapFrom(src => src.SkuDesc));
        }

    }
}
