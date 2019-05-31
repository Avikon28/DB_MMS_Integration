using SG.PO.Chino.DataModels.Outputmodels;

namespace SG.PO.Chino.CommandService.Core.OutputModels
{
    public class POChinoResult
    {
        public POChinoOutput POChino { get; set; }
        public bool Exists { get; set; }
        public bool Created { get; set; }
    }
}
