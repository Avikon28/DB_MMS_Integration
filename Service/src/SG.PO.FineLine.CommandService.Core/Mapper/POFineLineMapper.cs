using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.CommandService.Core.Mapper
{
    internal static class POFineLineMapper
    {
        private static IMapper Mapper = new MapperConfiguration(cfg => cfg.AddProfile<POFineLineProfile>()).CreateMapper();

        public static POFineLineOutput MaptoOutput(this POO entity)
        {
            return Mapper.Map<POFineLineOutput>(entity);
        }
    }
}
