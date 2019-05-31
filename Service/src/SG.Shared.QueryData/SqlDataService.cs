using System;
using System.Threading.Tasks;
using SG.Shared.QueryData.models;

namespace SG.Shared.QueryData
{
    class SqlDataService : IDataService
    {
        public Task<DataResult> GetItem<Tout, Tin>(string key,  string Index)
            where Tout : class
            where Tin : class
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateItem<T>(T model, string Index) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
