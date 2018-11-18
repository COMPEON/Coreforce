using Compos.Coreforce.Filter;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace Compos.Coreforce
{
    public interface ISalesforceClient
    {
        Task<List<T>> Get<T>();
        Task<List<T>> Get<T>(params Expression<Func<T, object>>[] fields);
        Task<List<T>> Get<T>(FilterCollection filterCollection);
        Task<List<T>> Get<T>(FilterCollection filterCollection, params Expression<Func<T, object>>[] fields);

        Task<List<dynamic>> Get(string sObject);
        Task<List<dynamic>> Get(string sObject, params string[] fields);
        Task<List<dynamic>> Get(string sObject, FilterCollection filterCollection);
        Task<List<dynamic>> Get(string sObject, FilterCollection filterCollection, params string[] fields);

        Task<T> GetById<T>(string id);
        Task<dynamic> GetById(string sObject, string id);

        Task Update<T>(T obj);
        Task Update<T>(T obj, params Expression<Func<T, object>>[] fields);
        Task Update(string sObject, dynamic obj);
        Task Update(string sObject, dynamic obj, params string[] fields);

        Task<string> Insert<T>(T obj);
        Task<string> Insert(string sObject, dynamic obj);

        Task Delete<T>(string id);
        Task Delete(string sObject, string id);

        Task<List<T>> Query<T>(string command);
        Task<List<dynamic>> Query(string command);
    }
}
