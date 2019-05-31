using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace SG.PO.Intertek.FileWriter.Helper
{
    internal static class Helper
    {
        public static string FlattenJson(string input, string arrayProperty)
        {

            DataTable dt = (DataTable)JsonConvert.DeserializeObject(input, (typeof(DataTable)));
            //Convert it to a JObject
            var unflattened = JsonConvert.DeserializeObject<JObject>(input);

            //Return a new array of items made up of the inner properties
            //of the array and the outer properties
            var flattened = ((JArray)unflattened[arrayProperty])
                .Select(item => new JObject(
                    unflattened.Properties().Where(p => p.Name != arrayProperty),
                    ((JObject)item).Properties()));

            //Convert it back to Json
            return JsonConvert.SerializeObject(flattened);
        }

        public static DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }



        public static void WriteFixedWidth(System.Xml.Linq.XElement CommandNode, DataTable Table, string outputStream)
        {
            StreamWriter Output = new StreamWriter(outputStream,true);
            int StartAt = CommandNode.Attribute("StartAt") != null ? int.Parse(CommandNode.Attribute("StartAt").Value) : 0;

            var positions = from c in CommandNode.Descendants("Position")
                            orderby int.Parse(c.Attribute("Start").Value) ascending
                            select new
                            {
                                Name = c.Attribute("Name").Value,
                                Start = int.Parse(c.Attribute("Start").Value) - StartAt,
                                Length = int.Parse(c.Attribute("Length").Value)
                            };

            int lineLength = positions.Last().Start + positions.Last().Length;



            foreach (DataRow row in Table.Rows)
            {
                StringBuilder line = new StringBuilder(lineLength);
                foreach (var p in positions)

                {
                    //check if the column exists in the datatable
                    if (row.Table.Columns.Contains(p.Name))
                    {
                        line.Insert(p.Start, (row[p.Name] ?? "").ToString().PadRight(p.Length, ' ')

                            );

                    }
                }
                Output.WriteLine(line.ToString());
            }
            Output.Flush();
        }
    }
}
