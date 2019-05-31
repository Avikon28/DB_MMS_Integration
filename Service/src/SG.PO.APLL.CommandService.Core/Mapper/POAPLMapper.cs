using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.APLL.CommandService.Core.Mapper;
using SG.PO.APLL.DataModel.Outputmodels;

namespace SG.PO.APLL.CommandService.Core
{
    internal static class POAPLMapper
    {
        private static IMapper Mapper = new MapperConfiguration(cfg => cfg.AddProfile<POAPLProfile>()).CreateMapper();

        public static POAPLLOutput MaptoOutput(this POO entity)
        {
            return Mapper.Map<POAPLLOutput>(entity);
        }





    }
}
