using SG.MMS.PO.Events;
using SG.PO.Chino.DataModels.Outputmodels;

namespace SG.PO.Chino.CommandService.Core.Mapper
{
    internal static class POChinoEventMapper
    {
        public static POChinoOutput MapEventtoOutput(this MMSPOEvent entity, POChinoOutput poChino)
        {
            poChino.UpdateChinoData(entity);
            return poChino;
        }
    }
}
