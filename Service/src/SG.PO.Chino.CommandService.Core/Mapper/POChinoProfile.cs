using SG.PO.Chino.CommandService.Core.Mapper.Helper;
using SG.PO.Chino.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using SG.PO.Chino.CommandService.Core.Helper;
using SG.MMS.QueryService.ODATA.Models.PO;

namespace SG.PO.Chino.CommandService.Core.Mapper
{
    public class POChinoProfile : AutoMapper.Profile
    {
        public POChinoProfile()
        {
            //po sku section
            CreateMap<POO, ICollection<POSkusOutput>>()
               .ConvertUsing(new POSkusConverter());

            // PO section
            CreateMap<POO, POChinoOutput>()
                   .ForMember(dest => dest.POSkus, opt => opt.MapFrom(src => src))
                   .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.PONumber))
                   .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.StatusCode))
                   .ForMember(dest => dest.PickupStart, opt => opt.MapFrom(src => src.OriginalDeliveryDate != null ? src.OriginalDeliveryDate.Value.ToString("MM/dd/yyyy HH:mm") : "00:00"))
                   .ForMember(dest => dest.DeliveryStart, opt => opt.MapFrom(src => src.OriginalDeliveryDate != null ? src.OriginalDeliveryDate.Value.ToString("MM/dd/yyyy HH:mm") : "00:00"))
                   .ForMember(dest => dest.DeliveryEnd, opt => opt.MapFrom(src => src.EstimatedArrivalDate != null ? src.EstimatedArrivalDate.Value.ToString("MM/dd/yyyy HH:mm") : "00:00"));                   
        }
    }
}
