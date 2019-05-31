using SG.MMS.PO.Events;
using SG.PO.Contempo.CommandService.Core.OutputModels;
using SG.PO.Contempo.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.Mapper
{
    internal static class POContempoEventMapper
    {
        public static POContempoOutput MapEventtoOutput(this MMSPOEvent entity, POContempoOutput poContempo)
        {
            poContempo.UpdatePOContempo(entity);
            return poContempo;
        }
    }
}
