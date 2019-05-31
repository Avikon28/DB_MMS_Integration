using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.CommandService.Core.outputmodels
{
    public class POFineLineProductResult
    {
        public POFineLineProductOutput POFLProductOutput { get; set; }
        public bool Exists { get; set; }
        public bool Created { get; set; }
    }
}