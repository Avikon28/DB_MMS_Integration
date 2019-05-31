using SG.PO.Contempo.DataModels.Outputmodels;

namespace SG.PO.Contempo.CommandService.Core.OutputModels
{
    public class POContempoProductResult
    {
        public POContempoProductOutput POCtmpProductOutput { get; set; }
        public bool Exists { get; set; }
        public bool Created { get; set; }
    }
}
