using SG.PO.Intertek.DataModels.Outputmodels;

namespace SG.PO.Intertek.CommandService.Core.Outputmodels
{
    public class POIntertekResult
    {
        public POIntertekOutput POIntertek { get; set; }
        public bool Exists { get; set; }
        public bool Created { get; set; }
    }
}
