using Compos.Coreforce.Filter;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;
using Compos.Coreforce.Models.Description;

namespace Compos.Coreforce
{
    public interface ISalesforceClient
    {
        Task<Description> GetDescriptionAsync<T>();

        Task<Description> GetDescriptionAsync(string sObject);

        Task<List<T>> GetAsync<T>();
        Task<List<T>> GetAsync<T>(params Expression<Func<T, object>>[] fields);
        Task<List<T>> GetAsync<T>(FilterCollection filterCollection);
        Task<List<T>> GetAsync<T>(FilterCollection filterCollection, params Expression<Func<T, object>>[] fields);

        Task<List<dynamic>> GetAsync(string sObject);
        Task<List<dynamic>> GetAsync(string sObject, params string[] fields);
        Task<List<dynamic>> GetAsync(string sObject, FilterCollection filterCollection);
        Task<List<dynamic>> GetAsync(string sObject, FilterCollection filterCollection, params string[] fields);

        Task<T> GetByIdAsync<T>(string id) where T : class;
        Task<dynamic> GetByIdAsync(string sObject, string id);

        Task UpdateAsync<T>(T obj);
        Task UpdateAsync<T>(T obj, params Expression<Func<T, object>>[] fields);
        Task UpdateAsync(string sObject, dynamic obj);
        Task UpdateAsync(string sObject, dynamic obj, params string[] fields);

        Task<string> InsertAsync<T>(T obj);
        Task<string> InsertAsync(string sObject, dynamic obj);

        Task DeleteAsync<T>(string id);
        Task DeleteAsync(string sObject, string id);

        Task<List<T>> QueryAsync<T>(string command);
        Task<List<dynamic>> QueryAsync(string command);
    }
}
