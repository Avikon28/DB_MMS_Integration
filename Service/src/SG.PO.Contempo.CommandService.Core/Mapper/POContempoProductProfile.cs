using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Contempo.DataModels.Outputmodels;


namespace SG.PO.APL.CommandService.Core.Mapper
{
    public class POContempoProductProfile : AutoMapper.Profile
    {
        public POContempoProductProfile()
        {
            CreateMap<POProduct, POContempoProductOutput>()
                  .ForMember(dest => dest.LabelType, opt => opt.MapFrom(src => src.LabelType))
                  .ForMember(dest => dest.LabelDescription, opt => opt.MapFrom(src => src.LabelDescription))
                  .ForMember(dest => dest.Class, opt => opt.MapFrom(src => src.Class))
                  .ForMember(dest => dest.CountryOfOrigin, opt => opt.MapFrom(src => src.CountryOfOrigin))
                  .ForMember(dest => dest.SubClass, opt => opt.MapFrom(src => src.SubClass))
                  .ForMember(dest => dest.APVendor, opt => opt.MapFrom(src => src.APVendor))
                  .ForMember(dest => dest.VendorSKUCode, opt => opt.MapFrom(src => src.Vendor_SKU_Code))
                  .ForMember(dest => dest.ClassLevelDesc, opt => opt.MapFrom(src => src.ClassDescription))
                  .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))
                  .ForMember(dest => dest.SkuDesc, opt => opt.MapFrom(src => src.SkuDescShrt))
                  .ForMember(dest => dest.SubClassLevelDesc, opt => opt.MapFrom(src => src.SubclassDescription));
        }

    }
}
