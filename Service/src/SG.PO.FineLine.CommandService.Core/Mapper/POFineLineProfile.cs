using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.FineLine.CommandService.Core.Mapper.Helper;
using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;
using System.Collections.Generic;

namespace SG.PO.FineLine.CommandService.Core.Mapper
{
    public class POFineLineProfile : AutoMapper.Profile
    {
        public POFineLineProfile()
        {
            //po sku section
            CreateMap<POO, ICollection<POFineLineSkuOutput>>()
               .ConvertUsing(new POSkusConverter());

            //FineLine
            CreateMap<POO, POFineLineOutput>()
               //.ForMember(dest => dest.Company, opt => opt.MapFrom(src => "030"))
               .ForMember(dest => dest.PurchaseOrder, opt => opt.MapFrom(src => src.PONumber))
               .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.CurrencyCode))
               .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.StatusCode))
               .ForMember(dest => dest.SubVendorNumber, opt => opt.MapFrom(src => src.SubVendorNumber))
               .ForMember(dest => dest.POSkus, opt => opt.MapFrom(src => src));
        }
    }
}
