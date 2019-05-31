using SG.MMS.PO.Events;
using SG.PO.Contempo.DataModels.Outputmodels;

namespace SG.PO.Contempo.CommandService.Core.Mapper
{
    internal static  class POContempoPOSkuEventMapper
    {
        public static POContempoOutput MapEventtoOutput(this MMSPOSkuEvent entity, POContempoOutput poCtmpo)
        {
            //POSkuevent will not come, so this will never be called
            //poCtmpo.UpdatePOContempoPoSkuData(entity);
            return poCtmpo;
        }
    }
}
