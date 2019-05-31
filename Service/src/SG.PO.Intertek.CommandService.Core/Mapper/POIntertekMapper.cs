using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Intertek.CommandService.Core.Mapper;
using SG.PO.Intertek.DataModels.Outputmodels;

namespace SG.PO.Intertek.CommandService.Core
{
    internal static class POIntertekMapper
    {
        private static IMapper Mapper = new MapperConfiguration(cfg => cfg.AddProfile<POIntertekProfile>()).CreateMapper();

        public static POIntertekOutput MaptoOutput(this POO entity)
        {
            return Mapper.Map<POIntertekOutput>(entity);
        }
    }
}
