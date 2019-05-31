using SG.PO.APLL.DataModel.Outputmodels;

namespace SG.PO.APLL.CommandService.Core.Outputmodels
{
    public class POProducrResult
    {
        public POProductOutput POProductOutput { get; set; }
        public bool Exists { get; set; }
        public bool Created { get; set; }
    }
}
