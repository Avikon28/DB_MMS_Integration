using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Chino.CommandService.Core.Mapper;
using SG.PO.Chino.CommandService.Core.OutputModels;
using SG.PO.Chino.DataModels.Outputmodels;
using System;

internal static class POChinoMapper
{
    private static IMapper Mapper = new MapperConfiguration(cfg => cfg.AddProfile<POChinoProfile>()).CreateMapper();

    public static POChinoOutput MaptoOutput(this POO entity)
    {
        return Mapper.Map<POChinoOutput>(entity);
    }
}