using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.APLL.DataModel.Outputmodels;


namespace SG.PO.APLL.CommandService.Core.Mapper
{
    public class POProductProfile : AutoMapper.Profile
    {
        public POProductProfile()
        {
            CreateMap<POProduct, POProductOutput>()
                  .ForMember(dest => dest.CountryOfOrigin, opt => opt.MapFrom(src => src.CountryOfOrigin))
                  .ForMember(dest => dest.ItemDescription, opt => opt.MapFrom(src => src.SkuDesc))
                  .ForMember(dest => dest.CasePackQty, opt => opt.MapFrom(src => src.MasterPackQuantity.HasValue ? src.MasterPackQuantity.ToString() : "0"))
                  .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class))
                  .ForMember(dest => dest.TariffCode, opt => opt.MapFrom(src => src.HTSCode))
                  .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.SubVendor))
                  .ForMember(dest => dest.VendorNumber, opt => opt.MapFrom(src => src.VendorName))
                  .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.ClassDescription))
                  .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassDescription))
                  .ForMember(dest => dest.DepartmentCode, opt => opt.MapFrom(src => src.Department));
        }

    }
}
