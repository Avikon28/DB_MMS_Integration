using SG.MMS.PO.Events;
using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.CommandService.Core.Mapper
{
    internal static class POFineLineEventMapper
    {
        public static POFineLineOutput MapEventtoOutput(this MMSPOEvent entity, POFineLineOutput pofineline)
        {
            pofineline.UpdatePOFineLine(entity);
            return pofineline;
        }
    }
}
