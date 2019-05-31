using Microsoft.Extensions.Options;
using SG.PO.Intertek.ProcessingService.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;

namespace SG.PO.Intertek.FileWriter
{
    public  class ProduceOutput
    {
        private readonly IOptions<OutputSettings> _outputSettings;
        public ProduceOutput(IOptions<OutputSettings> outputSettings)
        {

            _outputSettings = outputSettings;
        }

     

        public string WiteTxtFormat<T>(IList<T> document,XElement config) where T : class
        {
            try
            {

                DataTable dt = Helper.Helper.ToDataTable<T>(document);

                Helper.Helper.WriteFixedWidth(config, dt, _outputSettings.Value.OutputFilePath);

                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }


    }
}
