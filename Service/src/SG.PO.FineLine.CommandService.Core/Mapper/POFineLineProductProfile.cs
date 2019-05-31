using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.CommandService.Core.Mapper
{
    public class POFineLineProductProfile : AutoMapper.Profile
    {
        public POFineLineProductProfile()
        {
            CreateMap<POProduct, POFineLineProductOutput>()
                .ForMember(dest => dest.VendorNumber, opt => opt.MapFrom(src => src.APVendor))
                .ForMember(dest => dest.SubVendorNumber, opt => opt.MapFrom(src => src.SubVendor))
                .ForMember(dest => dest.SKUDescription, opt => opt.MapFrom(src => src.SkuDescShrt))
                .ForMember(dest => dest.VendorStyleNumber, opt => opt.MapFrom(src => src.VendorSkuCode))
                .ForMember(dest => dest.TicketType, opt => opt.MapFrom(src => src.LabelType))
                .ForMember(dest => dest.TicketDescription, opt => opt.MapFrom(src => src.LabelDescription))
                .ForMember(dest => dest.ClassID, opt => opt.MapFrom(src => src.Class))
                .ForMember(dest => dest.ClassDescription, opt => opt.MapFrom(src => src.ClassDescription))
                .ForMember(dest => dest.SubClassID, opt => opt.MapFrom(src => src.SubClass))
                .ForMember(dest => dest.SubClassDescription, opt => opt.MapFrom(src => src.SubclassDescription))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))
                .ForMember(dest => dest.ISOCountryCode, opt => opt.MapFrom(src => src.CountryOfOrigin));
        }
    }
}
