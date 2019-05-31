using System;
using System.Collections.Generic;
using System.Reflection;

namespace SG.Shared.QueryData.Helper
{
    public static class Helper
    {
        public static string GetPropertiestobeExpandedOn<T>(Microsoft.OData.Edm.IEdmModel metadata) where T : class
        {
            string expanededproperties = string.Empty;

            //get the object that the client has sent
            string[] arr1 = typeof(T).ToString().Split('.');
            string objectpassed = arr1[arr1.Length-1];

            var type = metadata.FindDeclaredType("SG.MMS.EventSync.Data.Entities.odata." + objectpassed);

            Type myType = type.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(type, null);
                if (propValue != null)
                {
                    var t = propValue.GetType().ToString();

                    if (t == "System.Collections.Generic.List`1[Microsoft.OData.Edm.IEdmProperty]")
                    {

                        var got = propValue as List<Microsoft.OData.Edm.IEdmProperty>;
                        if (got.Exists(y => y.GetType().ToString() == "Microsoft.OData.Edm.Csdl.CsdlSemantics.CsdlSemanticsNavigationProperty"))//checks if navigation properties exist
                        {
                            var navproperties = got.FindAll(y => y.GetType().ToString() == "Microsoft.OData.Edm.Csdl.CsdlSemantics.CsdlSemanticsNavigationProperty");
                            navproperties.ForEach(x => {
                                expanededproperties += x.Name + ",";
                            });
                        }



                    }
                }

            }


            return expanededproperties;

        }
    }
}
