using System;
using System.Globalization;
using SG.PO.Intertek.DataModels.Outputmodels;
namespace SG.PO.Intertek.CommandService.Core.Helper
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
        public static string ConvertToString(this bool? boolValue)
        {
            if (!boolValue.HasValue)
                return null;
            else
            {
                string strResult = (boolValue.Value == true) ? "Y" : "N";
                return strResult;
            }
        }
    }

}
