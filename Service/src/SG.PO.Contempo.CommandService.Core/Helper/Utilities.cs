using SG.PO.Contempo.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.Helper
{
    public static class Utilities
    {
        public static bool CheckPoStatus(this POContempoOutput POctmpOutput)
        {
            return POctmpOutput.StatusCode == "OP";
        }
    }
}
