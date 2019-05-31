using SG.MMS.PO.Events;
using SG.PO.APLL.DataModel.Outputmodels;

namespace SG.PO.APLL.CommandService.Core
{
    internal static  class POAPLPOSkuEventMapper
    {
        public static POAPLLOutput MapEventtoOutput(this MMSPOSkuEvent entity, POAPLLOutput poapl)
        {
            //POSkuevent will not come so this will never be called
            //poapl.UpdatePOAPLPoSkuData(entity);
            return poapl;
        }
    }
}
