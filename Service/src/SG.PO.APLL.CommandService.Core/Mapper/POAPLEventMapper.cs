using SG.MMS.PO.Events;
using SG.PO.APLL.DataModel.Outputmodels;

namespace SG.PO.APLL.CommandService.Core.Mapper
{
    internal static class POAPLEventMapper
    {
        public static POAPLLOutput MapEventtoOutput(this MMSPOEvent entity, POAPLLOutput poapl)
        {
            poapl.UpdatePOAPL(entity);
            return poapl;
        }
    }
}
