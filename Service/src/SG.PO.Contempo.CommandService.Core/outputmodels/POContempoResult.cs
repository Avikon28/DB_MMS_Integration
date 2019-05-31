using SG.PO.Contempo.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.OutputModels
{
    public class POContempoResult
    {
        public POContempoOutput poContempo { get; set; }
        public bool Exists { get; set; }
        public bool Created { get; set; }
    }
}
