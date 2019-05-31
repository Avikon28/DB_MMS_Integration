using SG.MMS.PO.Events;
using SG.PO.Intertek.DataModels.Outputmodels;

namespace SG.PO.Intertek.CommandService.Core.Mapper
{
    internal static class POIntertekEventMapper
    {
        public static POIntertekOutput MapEventtoOutput(this MMSPOEvent entity, POIntertekOutput poIntertek)
        {
            poIntertek.UpdateIntertekData(entity);
            return poIntertek;
        }
    }
}
