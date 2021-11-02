using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;

namespace Epsilon.Model.SharedDic
{
    public class SharedDicService : IService
    {
        private readonly SharedDicModel _model;

        public SharedDicService(SharedDicModel model)
        {
            _model = model;
        }

        public void Delete(string key)
        {
            _model.Delete(key);
        }

       
        public T Get<T>( string key)
        {
            var strVal = _model.Get(key);
            return JsonSerializer.Deserialize<T>(strVal);
        }

        public T CreateOrUpdate<T>( string key, T value, ExpirationDateRef dateRef, int expirationTime)
        {
            var json = JsonSerializer.Serialize(value, new JsonSerializerOptions{ WriteIndented = true});
            _model.CreateOrUpdate(key, json, dateRef, expirationTime);
            return value;
        }
    }

}
