using System.Collections.Generic;

namespace SG.PO.FineLine.ProcessingService.Services
{
    public class OutputSettings
    {
        public string Type { get; set; }
        public string OutputFilePath { get; set; }
        public List<string> MembersToInclude { get; set; }
        public string OutputFileNameDateFormat { get; set; }
    }
}
