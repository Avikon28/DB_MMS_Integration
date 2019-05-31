using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Contempo.DataModels.Outputmodels;

namespace SG.PO.Contempo.CommandService.Core.Mapper
{
    internal static class POContempoMapper
    {
        private static IMapper Mapper = new MapperConfiguration(cfg => cfg.AddProfile<POContempoProfile>()).CreateMapper();

        public static POContempoOutput MaptoOutput(this POO entity)
        {
            return Mapper.Map<POContempoOutput>(entity);
        }
    }
}
