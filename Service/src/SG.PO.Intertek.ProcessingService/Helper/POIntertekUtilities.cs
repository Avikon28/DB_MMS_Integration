using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SG.PO.Intertek.DataModels.Outputmodels;
using SG.PO.Intertek.ProcessingService.Helper;
using SG.PO.Intertek.ProcessingService.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace SG.PO.Intertek.ProcessingService
{
    public class POIntertekUtilities
    {
        private readonly ILogger _logger;
        private readonly IOptions<OutputSettings> _outputSettings;

        public POIntertekUtilities(ILogger<ElasticWriter> logger, IOptions<OutputSettings> outputSettings)
        {

            _logger = logger;
            _outputSettings = outputSettings;
        }

        public string WriteTxtFormat<T>(IList<T> document, XElement config, string outputFileSavePath) where T : class
        {
            DataTable dt = ToDataTable<T>(document);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                _logger.LogInformation($"Output successfully written to {outputFileSavePath}.");
                WriteFixedWidth(config, dt, outputFileSavePath);
                return "Success";
            }
            else
            {
                _logger.LogInformation($"No output written; no new or changed data found.");
                return "No Records to write";
            }
        }

        //mehod to compare 2 Intertek objects and return the POskulines that would be sent back
        public POIntertekOutput CompareIntertekData(POIntertekOutput poskusOutputcurrent, POIntertekOutput poskusOutputprev)
        {
            try
            {
                bool poupdated = false;
                int propertyCount = typeof(POIntertekOutput).GetProperties().Length;
                CompareLogic basicComparison = new CompareLogic()
                {
                    Config = new ComparisonConfig()
                    {
                        MaxDifferences = propertyCount
                    }
                };
                List<Difference> diffs = basicComparison.Compare(poskusOutputcurrent, poskusOutputprev).Differences;

                //deliberately set the activty code for poskus to null. later it will be "A"/"U" as applicable
                if (poskusOutputcurrent.POSkus != null && poskusOutputcurrent.POSkus.Count > 0)
                {
                    poskusOutputcurrent.POSkus.ForEach(y =>
                    {
                        y.ActivityCode = null;
                    });
                }
                else
                {
                    _logger.LogInformation($"CompareIntertekData - Skipping output for PO - {poskusOutputcurrent.PONumber}; no SKUs were found.");
                    return new POIntertekOutput();
                }

                if (diffs != null && diffs.Count() > 0)
                {
                    //check if there is a difference on PO level/then tag all POSkus as "U"
                    if (diffs.Where(x => x.ParentPropertyName == string.Empty).Count() > 0)
                    {
                        poupdated = true;
                    }

                    //go to the POSKUs collection and figure if there has been a change

                    poskusOutputcurrent.POSkus.ForEach(y =>
                    {
                        //Look for the prior version sent
                        var poskutobecompared = poskusOutputprev.POSkus.Find(posku => posku.SKU == y.SKU);

                        if (poskutobecompared == null)
                        {
                            //Never sent before.  Only send if the posku status isn't canceled/voided
                            if (y.StatusCode != "CN" && y.StatusCode != "VD")
                            {
                                y.ActivityCode = "A";
                            }
                        }
                        else
                        {
                            if ((y.StatusCode == "CN" || y.StatusCode == "VD"))
                            {
                                //This one is canceled/voided.  If it was open the last time we sent it, send as a "D".  Otherwise, we'll skip it
                                if (poskutobecompared.StatusCode == "OP")
                                {
                                    y.ActivityCode = "D";
                                }
                            }
                            else if (poupdated)
                            {
                                //Parent PO was modified, so all the child skus are considered modified
                                y.ActivityCode = "U";
                            }
                            else
                            {
                                //Parent PO hasn't changed.  See if the sku line was modified
                                int propertyCountskus = typeof(POIntertekOutput).GetProperties().Length;
                                CompareLogic basicComparisonposkus = new CompareLogic()
                                { Config = new ComparisonConfig() { MaxDifferences = propertyCountskus } };
                                List<Difference> diffposkus = basicComparison.Compare(y, poskutobecompared).Differences;

                                if (diffposkus != null && diffposkus.Any(d => d.PropertyName != "ActivityCode"))//ignore the activitycode for comparison
                                {
                                    y.ActivityCode = "U";
                                }
                            }

                        }
                    });
                }
                var finalupdated = poskusOutputcurrent.POSkus.Where(posku => posku.ActivityCode != null);
                if (finalupdated != null)
                {
                    POIntertekOutput POIntertekOutputtobesent = new POIntertekOutput
                    {
                        PONumber = poskusOutputcurrent.PONumber,
                        POSkus = finalupdated.ToList(),
                        CancelDate = poskusOutputcurrent.CancelDate,
                        DeliveryDate = poskusOutputcurrent.DeliveryDate,
                        EmployeeId = poskusOutputcurrent.EmployeeId,
                        LocationNumber = poskusOutputcurrent.LocationNumber,
                        ShipDate = poskusOutputcurrent.ShipDate,
                        VendorName = poskusOutputcurrent.VendorName,
                        SubVendorNumber = poskusOutputcurrent.SubVendorNumber,
                        DistributorId = poskusOutputcurrent.DistributorId,
                        LOB = poskusOutputcurrent.LOB
                    };
                    return POIntertekOutputtobesent;
                }

                return new POIntertekOutput();
            }
            catch (Exception ex)
            {
                _logger.LogError("Method Name -- CompareIntertekData : {Reason}", ex.Message);
                return new POIntertekOutput();
            }
        }

        internal DataTable ToDataTable<T>(IList<T> data)
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

        internal void WriteFixedWidth(System.Xml.Linq.XElement CommandNode, DataTable Table, string outputStrmFilePath)
        {
            try
            {
                int StartAt = CommandNode.Attribute("StartAt") != null ? int.Parse(CommandNode.Attribute("StartAt").Value) : 0;

                var positions = from c in CommandNode.Descendants("Position")
                                orderby int.Parse(c.Attribute("Start").Value) ascending
                                select new
                                {
                                    Name = c.Attribute("Name").Value,
                                    Start = int.Parse(c.Attribute("Start").Value) - StartAt,
                                    Length = int.Parse(c.Attribute("Length").Value),
                                    DefaultValue = c.Attribute("DefaultValue") != null ? c.Attribute("DefaultValue").Value : string.Empty,//optional,
                                    Type = c.Attribute("Type") != null ? c.Attribute("Type").Value : string.Empty,//optional
                                    ExistsInTable = Table.Columns.Contains(c.Attribute("Name").Value)
                                };

                int lineLength = positions.Last().Start + positions.Last().Length;

                using (var stream = new StreamWriter(outputStrmFilePath, true))
                {
                    // Use stream
                    foreach (DataRow row in Table.Rows)
                    {
                        StringBuilder line = new StringBuilder(lineLength);
                        foreach (var p in positions)

                        {
                            //check if the column exists in the datatable
                            if (p.ExistsInTable)
                            {
                                if (!string.IsNullOrEmpty(p.DefaultValue))
                                {
                                    line.Insert(p.Start, (p.DefaultValue ?? "").ToString().PadRight(p.Length, ' '));
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(p.Type) && p.Type == "N" && !string.IsNullOrEmpty(row[p.Name].ToString()))
                                    {
                                        int lengthallotted = p.Length - p.Start;
                                        string withzeroes = string.Empty;
                                        //check if the data is decimal, remove the decimal
                                        //padd zeroes to the right
                                        if (row[p.Name].ToString().Contains("."))
                                        {
                                            string decimalremoved = row[p.Name].ToString().Replace(".", string.Empty);
                                            withzeroes = string.Empty.PadLeft(lengthallotted - decimalremoved.Length, '0') + decimalremoved.ToString();
                                            line.Insert(p.Start, withzeroes);
                                        }
                                        else
                                        {
                                            withzeroes = lengthallotted > row[p.Name].ToString().Length ? string.Empty.PadLeft(lengthallotted - row[p.Name].ToString().Length, '0') + row[p.Name].ToString() : row[p.Name].ToString();
                                            //withzeroes = string.Empty.PadLeft(lengthallotted - row[p.Name].ToString().Length, '0') + row[p.Name].ToString();
                                            line.Insert(p.Start, withzeroes);
                                        }
                                    }
                                    else
                                    {
                                        line.Insert(p.Start, (row[p.Name] ?? "").ToString().PadRight(p.Length, ' '));
                                    }
                                }
                            }
                            else
                            {
                                //log an error and exit
                                _logger.LogError("Error while generating POAPL file. Column name mismatch for--{column}", p.Name);
                                break;
                            }
                        }
                        stream.WriteLine(line.ToString());
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Method Name -- WriteFixedWidth: {Reason}", ex.Message);
            }
            
        }
    }
}
