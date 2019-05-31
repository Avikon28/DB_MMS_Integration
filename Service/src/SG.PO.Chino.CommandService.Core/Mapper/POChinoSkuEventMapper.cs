using SG.MMS.PO.Events;
using SG.PO.Chino.CommandService.Core.Mapper;
using SG.PO.Chino.DataModels.Outputmodels;

namespace SG.PO.Chino.CommandService.Core
{
    internal static class POChinoPOSkuEventMapper
    {
        public static POChinoOutput MapEventtoOutput(this MMSPOSkuEvent entity, POChinoOutput poChino)
        {
            //POSkuevent will not come so this will never be called
            //poChino.UpdatePOChinoPoSkuData(entity);
            return poChino;
        }
    }
}
