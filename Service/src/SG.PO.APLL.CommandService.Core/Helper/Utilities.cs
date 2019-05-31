using SG.PO.APLL.DataModel.Outputmodels;
using System;
using System.Globalization;

namespace SG.PO.APLL.CommandService.Core.Helper
{
    public static class Utilities
    {
        public static string FormatTo(this string datetime)
        {
            if(!string.IsNullOrEmpty(datetime)) 
            {
                DateTime date = DateTime.ParseExact(datetime, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                return date.ToString("yyyyMMdd"); 
                
            }
            else
            {
                return string.Empty;
            }

            
        }

        public static bool CheckPoStatus(this POAPLLOutput output)
        {
            return output.StatusCode=="OP";
        }
    }
}
