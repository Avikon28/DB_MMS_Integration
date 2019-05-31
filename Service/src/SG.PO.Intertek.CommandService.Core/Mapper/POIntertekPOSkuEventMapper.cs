using SG.MMS.PO.Events;
using SG.PO.Intertek.CommandService.Core.Mapper;
using SG.PO.Intertek.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Intertek.CommandService.Core.Mapper
{
    internal static class POIntertekPOSkuEventMapper
    {
        public static POIntertekOutput MapEventtoOutput(this MMSPOSkuEvent entity, POIntertekOutput pointertek)
        {
            //POSkuevent will not come so this will never be called
            //pointertek.UpdatePOIntertekPoSkuData(entity);
            return pointertek;
        }
    }
}
