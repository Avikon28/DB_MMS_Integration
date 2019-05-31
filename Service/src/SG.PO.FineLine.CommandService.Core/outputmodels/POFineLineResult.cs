using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.CommandService.Core.outputmodels
{
    public class POFineLineResult
    {
        public POFineLineOutput POFineLine { get; set; }
        public bool Exists { get; set; }
        public bool Created { get; set; }
    }
}
