using SG.MMS.PO.Events;
using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.CommandService.Core.Mapper
{
    internal static class POFineLinePOSkuEventMapper
    {
        public static POFineLineOutput MapEventtoOutput(this MMSPOSkuEvent entity, POFineLineOutput pofineline)
        {
            //POSkuevent will not come so this will never be called
            //pofineline.UpdatePOFineLinePoSkuData(entity);
            return pofineline;
        }
    }
}
