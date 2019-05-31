using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Contempo.CommandService.Core.Mapper.Helper;
using SG.PO.Contempo.DataModels.Outputmodels;
using System.Collections.Generic;

namespace SG.PO.Contempo.CommandService.Core.Mapper
{
    public class POContempoProfile : AutoMapper.Profile
    {
        public POContempoProfile()
        {

            //po sku section
            CreateMap<POO, IList<POContempoSkuOutput>>()
               .ConvertUsing(new POContempoSkusConverter());

            CreateMap<POO, POContempoOutput>()
                .ForMember(dest => dest.PONumber, opt => opt.MapFrom(src => src.PONumber))
                .ForMember(dest => dest.SubVendor, opt => opt.MapFrom(src => src.SubVendorNumber))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.StatusCode))
                .ForMember(dest => dest.POSkus, opt => opt.MapFrom(src => src));

        }
    }
}
