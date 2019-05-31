using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;
using System;
using System.Globalization;

namespace SG.PO.FineLine.CommandService.Core.Helper
{
    public static class Utilities
    {
        public static string FormatTo(this string datetime)
        {
            if (!string.IsNullOrEmpty(datetime))
            {
                DateTime date = DateTime.ParseExact(datetime, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                return date.ToString("yyyyMMdd");
            }
            else
            {
                return string.Empty;
            }
        }
        
        public static bool CheckPoStatus(this POFineLineOutput POFineLineOutput)
        {
            return POFineLineOutput.StatusCode == "OP";
        }
    }
}
