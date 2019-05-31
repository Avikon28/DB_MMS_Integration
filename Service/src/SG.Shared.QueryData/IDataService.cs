using SG.Shared.QueryData.models;
using System.Threading.Tasks;

namespace SG.Shared.QueryData
{
    public interface IDataService
    {
        Task<DataResult> GetItem<Tout, Tin>(string key, string Index) where Tout : class where Tin : class;

        Task<T> UpdateItem<T>(T model, string Index) where T : class;
    }
}
