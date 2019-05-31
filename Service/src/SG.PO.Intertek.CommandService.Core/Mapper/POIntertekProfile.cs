using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Intertek.CommandService.Core.Mapper.Helper;
using SG.PO.Intertek.DataModels.Outputmodels;
using System.Collections.Generic;

namespace SG.PO.Intertek.CommandService.Core.Mapper
{
    public class POIntertekProfile : AutoMapper.Profile
    {
        public POIntertekProfile()
        {
            //po sku section
            CreateMap<POO, ICollection<POIntertekSKUOutput>>()
               .ConvertUsing(new POSkusConverter());

            // po section
            CreateMap<POO, POIntertekOutput>()
                   .ForMember(dest => dest.POSkus, opt => opt.MapFrom(src => src))
                   .ForMember(dest => dest.LOB, opt => opt.MapFrom(src => src.LOB))
                   .ForMember(dest => dest.DeliveryDate, opt => opt.MapFrom(src => src.DeliveryDate != null ? src.DeliveryDate.Value.ToString("yyyyMMdd") : string.Empty))
                   .ForMember(dest => dest.CancelDate, opt => opt.MapFrom(src => src.CancelDate != null ? src.CancelDate.Value.ToString("yyyyMMdd") : string.Empty))
                   .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeID))
                   .ForMember(dest => dest.LocationNumber, opt => opt.MapFrom(src => src.LocationNumber))
                   .ForMember(dest => dest.PONumber, opt => opt.MapFrom(src => src.PONumber))
                   .ForMember(dest => dest.ShipDate, opt => opt.MapFrom(src => src.ShipDate != null ? src.ShipDate.Value.ToString("yyyyMMdd") : string.Empty))
                   .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.SubVendor.VendName))
                   .ForMember(dest => dest.SubVendorNumber, opt => opt.MapFrom(src => src.SubVendor.VendCode))
                   .ForMember(dest => dest.DistributorId, opt => opt.MapFrom(src => src.DistributorId))
                   .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                   .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.StatusCode));
        }
    }
}
