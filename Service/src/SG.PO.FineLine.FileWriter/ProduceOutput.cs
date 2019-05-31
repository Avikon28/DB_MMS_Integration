using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SG.PO.FineLine.FileWriter.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace SG.PO.FineLine.FileWriter
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
