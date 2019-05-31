namespace SG.Shared.QueryData.models
{
    public class ElasticResult<T> : DataResult where T : class 
    {
        public T Output { get; set; }


       
    }
    public class DBResult<T> : DataResult where T : class
    {
        public T Output { get; set; }



    }
    public class DataResult
    {
        public bool Exists { get; set; }
    }
}
