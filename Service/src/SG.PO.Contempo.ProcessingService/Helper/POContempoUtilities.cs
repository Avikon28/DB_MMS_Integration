using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SG.PO.Contempo.DataModels.Outputmodels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.PO.Contempo.ProcessingService.Services;
using System.Xml.Linq;

namespace SG.PO.Contempo.ProcessingService.Helper
{
    public class POContempoUtilities
    {
        private readonly ILogger _logger;
        private readonly IOptions<OutputSettings> _outputSettings;

        public POContempoUtilities(ILogger<ElasticWriter> logger, IOptions<OutputSettings> outputSettings)
        {
            _logger = logger;
            _outputSettings = outputSettings;
        }

        public string WriteTxtFormat<T>(IList<T> document, XElement config, string outputFileSavePath) where T : class
        {
            DataTable dt = ToDataTable<T>(document);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                WriteFixedWidth(config, dt, outputFileSavePath);
                _logger.LogInformation($"Output successfully written to {outputFileSavePath}.");
                return "Success";
            }
            else
            {
                _logger.LogInformation($"No output written; no new or changed data found.");
                return "No Records to write";
            }
        }

        public DataTable ToDataTable<T>(IList<T> data)
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

        public void WriteFixedWidth(System.Xml.Linq.XElement CommandNode, DataTable Table, string outputStrmFilePath)
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
                                    ExistsInTable = Table.Columns.Contains(c.Attribute("Name").Value),
                                    Type = c.Attribute("Type") != null ? c.Attribute("Type").Value : string.Empty //optional
                                };

                int lineLength = positions.Last().Start + positions.Last().Length;

                using (StreamWriter Output = new StreamWriter(outputStrmFilePath, true))
                {
                    foreach (DataRow row in Table.Rows)
                    {
                        StringBuilder line = new StringBuilder(lineLength);
                        foreach (var p in positions)
                        {
                            //check if the column exists in the datatable
                            string rowData = string.Empty;
                            if (p.ExistsInTable)
                            {
                                if (!string.IsNullOrEmpty(p.Type) && p.Type == "N" && !string.IsNullOrEmpty(row[p.Name].ToString()))
                                {
                                    int lengthAllotted = p.Length - p.Start;
                                    string withzeroes = string.Empty;
                                    //check if the data is decimal, remove the decimal and padd zeroes to the right
                                    if (row[p.Name].ToString().Contains("."))
                                    {
                                        string decimalremoved = row[p.Name].ToString();
                                        withzeroes = string.Empty.PadLeft(lengthAllotted - decimalremoved.Length, '0') + decimalremoved.ToString();
                                    }
                                    else
                                    {
                                        withzeroes = lengthAllotted > row[p.Name].ToString().Length ? string.Empty.PadLeft(lengthAllotted - row[p.Name].ToString().Length, '0') + row[p.Name].ToString() : row[p.Name].ToString();
                                        //withzeroes = string.Empty.PadLeft(lengthAllotted - row[p.Name].ToString().Length, '0') + row[p.Name].ToString();
                                    }
                                    line.Append(withzeroes).Append("\t");
                                }
                                else
                                {
                                    rowData = row[p.Name] != null ? Convert.ToString(row[p.Name]) : "";
                                    int dataLength = (p.Length - p.Start) > rowData.Length ? rowData.Length : (p.Length - p.Start);
                                    line.Append(rowData.Substring(0, dataLength)).Append("\t");
                                }                                
                            }
                            else
                            {
                                line.Append(rowData).Append("\t");
                                _logger.LogError("Error while generating PO Contempo file---Column name mismatch--{column} - " + p.Name);
                                break;
                            }
                        }
                        Output.WriteLine(line.ToString());
                    }
                    //Output.Flush();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Method Name -- WriteFixedWidth: {Reason}, {InnerEx}", ex?.Message, ex?.InnerException?.Message);
            }
        }

        /// <summary>
        /// Method to compare 2 PO Contempo objects and return the POskulines that contains the data we want in our final output
        /// </summary>
        public POContempoOutput CompareContempoData(POContempoOutput poskusOutputcurrent, POContempoOutput poskusOutputprev, List<string> memberstoInclude)
        {
            try
            {
                //if the PO is closed, we won't be including it, even if it's changed
                if (poskusOutputcurrent.StatusCode == "CL")
                    return new POContempoOutput();

                //Deliberately set the activty code for poskus to null. It will then 
                //be set appropriately on each sku we decide we want to include
                if (poskusOutputcurrent.POSkus != null && poskusOutputcurrent.POSkus.Count > 0)
                {
                    poskusOutputcurrent.POSkus.ForEach(y =>
                    {
                        y.ActivityCode = null;
                    });
                }
                else
                {
                    _logger.LogInformation($"CompareContempoData - Skipping output for PO - {poskusOutputcurrent.PONumber}; no SKUs were found.");
                    return new POContempoOutput();
                }

                int propertyCount = typeof(POContempoOutput).GetProperties().Length;
                CompareLogic basicComparison = new CompareLogic()
                {
                    Config = new ComparisonConfig()
                    {
                        MaxDifferences = propertyCount,
                        MembersToInclude = memberstoInclude
                    }
                };
                List<Difference> diffs = basicComparison.Compare(poskusOutputcurrent, poskusOutputprev).Differences;

                //None of the values we care about have changed on this PO, so we won't send it.
                if (diffs == null || diffs.Count() == 0)
                {
                    bool poUpdated = false;
                    poskusOutputcurrent.POSkus.ForEach(y => CompareContempoPOSkuData(y, poUpdated, poskusOutputprev, basicComparison));
                    var finalupdatedprod = poskusOutputcurrent.POSkus.Where(posku => posku.ActivityCode != null);
                    if (finalupdatedprod == null || (finalupdatedprod != null && finalupdatedprod.Count() == 0))
                        return new POContempoOutput();
                }

                ///See if it's been cancelled at the PO level
                if (poskusOutputcurrent.StatusCode == "CN" || poskusOutputcurrent.StatusCode == "VD")
                {
                    //If it was previously cancelled, don't send
                    if (poskusOutputprev.StatusCode == "CN" || poskusOutputprev.StatusCode == "VD")
                        return new POContempoOutput();

                    //Tag first sku with an activity code of "C" and leave the rest null 
                    //(we'll only need to send one line for all skus on the PO in this scenario)
                    var first = poskusOutputcurrent.POSkus.FirstOrDefault();
                    if (first != null)
                        first.ActivityCode = "C";
                }
                else
                {
                    //Determine if there were any differences on PO level, and then validate each sku line for inclusion
                    bool poUpdated = (diffs?.Where(x => x.ParentPropertyName == string.Empty).Count() > 0);
                    poskusOutputcurrent.POSkus.ForEach(y => CompareContempoPOSkuData(y, poUpdated, poskusOutputprev, basicComparison));
                }

                //Now that we're done tagging, all the ones with a non-null ActivityCode are the ones we want in our output file
                var finalupdated = poskusOutputcurrent.POSkus.Where(posku => posku.ActivityCode != null);
                if (finalupdated == null || (finalupdated != null && finalupdated.Count() == 0))
                    return new POContempoOutput();

                var POContempoOutputtobesent = new POContempoOutput
                {
                    //Company = poskusOutputcurrent.Company,
                    PONumber = poskusOutputcurrent.PONumber,
                    CurrencyCode = poskusOutputcurrent.CurrencyCode,
                    StatusCode = poskusOutputcurrent.StatusCode,
                    SubVendor = poskusOutputcurrent.SubVendor,
                    POSkus = finalupdated.ToList()
                };
                return POContempoOutputtobesent;
            }
            catch (Exception ex)
            {
                _logger.LogError("Method Name -- CompareContempoData: PO-{PONumber}, {Reason}, {InnerEx}", poskusOutputcurrent.PONumber, ex?.Message, ex?.InnerException.ToString());
                return new POContempoOutput();
            }
        }

        /// <summary>
        /// For a given posku, this will determine if it should be sent through and, if so, sets the ActivityCode appropriately
        /// </summary>
        private void CompareContempoPOSkuData(POContempoSkuOutput currentSku, bool parentPOUpdated, POContempoOutput priorPO, CompareLogic comparisson)
        {
            //grab the previously sent version for this sku
            var poskutobecompared = priorPO.POSkus != null ? priorPO.POSkus.Find(posku => posku.SKU == currentSku.SKU) : null;

            if (poskutobecompared == null)
            {
                //Never sent before.  Only send if its status is open
                if (currentSku.StatusCode == "OP")
                    currentSku.ActivityCode = "";
            }
            else if (currentSku.StatusCode == "CN" || currentSku.StatusCode == "VD")
            {
                //Cancelled/voided.  Send as "X", but only if it wasn't cancelled or voided when we last sent it
                if (poskutobecompared.StatusCode != "CN" && poskutobecompared.StatusCode != "VD")
                    currentSku.ActivityCode = "X";
            }
            else if (parentPOUpdated)
            {
                //Parent PO was modified, so all the child skus are considered modified
                currentSku.ActivityCode = "";
            }
            else
            {
                //Parent PO hasn't changed.  See if this sku line was itself modified
                var diffposkus = comparisson.Compare(currentSku, poskutobecompared).Differences;
                var diffpoproduct = comparisson.Compare(currentSku.POProduct, poskutobecompared.POProduct).Differences;

                if (diffposkus?.Count > 0 || diffpoproduct?.Count > 0)
                    currentSku.ActivityCode = "";
            }
        }

        //Method to compare 2 Contempo objects and return the POskulines that would be sent back
        //public POContempoOutput CompareContempoData(POContempoOutput poskusOutputcurrent, POContempoOutput poskusOutputprev)
        //{
        //    try
        //    {
        //        bool poupdated = false;
        //        int propertyCount = typeof(POContempoOutput).GetProperties().Length;
        //        CompareLogic basicComparison = new CompareLogic()
        //        { Config = new ComparisonConfig() { MaxDifferences = propertyCount } };
        //        List<Difference> diffs = basicComparison.Compare(poskusOutputcurrent, poskusOutputprev).Differences;

        //        //deliberately set the activty code for poskus to null. later it will be "A"/"U", empty as applicable
        //        poskusOutputcurrent.POSkus.ForEach(y =>
        //        {
        //            y.ActivityCode = null;
        //        });

        //        if (diffs != null && diffs.Count() > 0)
        //        {
        //            //check if there is a difference on PO level/then tag all POSkus as "U"
        //            if (diffs.Where(x => x.ParentPropertyName == string.Empty).Count() > 0)
        //            {
        //                poupdated = true;
        //            }

        //            //go to the POSKUs collection and figure if there has been a change

        //            poskusOutputcurrent.POSkus.ForEach(y =>
        //            {
        //                /*
        //                 * The activity code being set on each SKU during CompareContempOutput looks like a blind copy from APLL, but it's actually different. 
        //                    If the current statuscode on the SKU is either "CN" or "VD":
        //                        If there is a previous version:
        //                            Set the activity code to "C" if the status of the parent PO is either "CN" or "VD"
        //                            Otherwise set the activity code to "X"
        //                        Otherwise, do not include in output
        //                    Otherwise, if there is no prior version, include the sku but leave activitycode blank
        //                    Otherwise, only include the sku if there are changes from the prior version, and leave activitycode blank
        //                 */

        //                //Look for the prior version sent
        //                var poskutobecompared = poskusOutputprev.POSkus.Find(posku => posku.SKU == y.SKU);

        //                if (y.StatusCode == "CN" || y.StatusCode == "VD")
        //                {
        //                    if (poskutobecompared != null)
        //                    {
        //                        if (poskusOutputcurrent.StatusCode == "CN" || poskusOutputcurrent.StatusCode == "VD")
        //                        {
        //                            y.ActivityCode = "C";
        //                        }
        //                        else
        //                        {
        //                            y.ActivityCode = "X";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (poupdated)
        //                        {
        //                            y.ActivityCode = "";
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (poskutobecompared == null)
        //                    {
        //                        y.ActivityCode = null;
        //                    }
        //                    else
        //                    {
        //                        //check if a POSku has been modified
        //                        int propertyCountskus = typeof(POContempoSkuOutput).GetProperties().Length;
        //                        CompareLogic basicComparisonposkus = new CompareLogic()
        //                        { Config = new ComparisonConfig() { MaxDifferences = propertyCountskus } };
        //                        List<Difference> diffposkus = basicComparison.Compare(y, poskutobecompared).Differences;

        //                        if (diffposkus != null && diffposkus.Any(d => d.PropertyName != "ActivityCode"))//ignore the activitycode for comparison
        //                        {
        //                            y.ActivityCode = "";
        //                        }
        //                    }

        //                    if (poupdated)
        //                    {
        //                        //Parent PO was modified, so all the child skus are considered modified
        //                        y.ActivityCode = "";
        //                    }
        //                }
        //            });
        //        }                

        //        var finalupdated = poskusOutputcurrent.POSkus.Where(posku => posku.ActivityCode != null);
        //        if (finalupdated != null && finalupdated.Count() > 0)
        //        {
        //            POContempoOutput POCtmpOutputtobesent = new POContempoOutput
        //            {
        //                CurrencyCode = poskusOutputcurrent.CurrencyCode,
        //                PONumber = poskusOutputcurrent.PONumber,
        //                SubVendor = poskusOutputcurrent.SubVendor,
        //                POSkus = finalupdated.ToList(),
        //                StatusCode = poskusOutputcurrent.StatusCode
        //            };
        //            return POCtmpOutputtobesent;
        //        }

        //        return new POContempoOutput();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(null, ex, ex.Message);
        //        return new POContempoOutput();
        //    }
        //}
    }
}
