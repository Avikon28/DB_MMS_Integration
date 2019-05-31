using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.APLL.CommandService.Core.Mapper.Helper;
using SG.PO.APLL.DataModel.Outputmodels;
using System.Collections.Generic;

namespace SG.PO.APLL.CommandService.Core.Mapper
{
    public class POAPLProfile : AutoMapper.Profile
    {
        public POAPLProfile()
        {



            //po sku section
            CreateMap<POO, ICollection<POSkusOutput>>()
               .ConvertUsing(new POSkusConverter());

            //po section
            CreateMap<POO, POAPLLOutput>()
                   .ForMember(dest => dest.POSkus, opt => opt.MapFrom(src => src))
                   .ForMember(dest => dest.WarehouseDueDate, opt => opt.MapFrom(src => src.DeliveryDate!=null ? src.DeliveryDate.Value.ToString("yyyyMMdd") : string.Empty))
                   .ForMember(dest => dest.EarlyShipDate, opt => opt.MapFrom(src => src.ShipDate != null ? src.ShipDate.Value.ToString("yyyyMMdd") : string.Empty))
                   .ForMember(dest => dest.LastShipDate, opt => opt.MapFrom(src => src.CancelDate != null ? src.CancelDate.Value.ToString("yyyyMMdd") : string.Empty))
                   .ForMember(dest => dest.StoreCode, opt => opt.MapFrom(src => src.LocationNumber))
                   .ForMember(dest => dest.BuyerCode, opt => opt.MapFrom(src => src.EmployeeID))
                   .ForMember(dest => dest.VendName, opt => opt.MapFrom(src => src.SubVendor.VendName))
                   .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.StatusCode))
                   .ForMember(dest => dest.LOB, opt => opt.MapFrom(src => src.LOB))
                   .ForMember(dest => dest.VendorShipDate, opt => opt.MapFrom(src => src.CancelDate != null ? src.ShipDate.Value.ToString("yyyyMMdd") : string.Empty));
        }
        
    }
}
