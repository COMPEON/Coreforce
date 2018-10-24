using Compos.Coreforce.Models;
using Compos.Coreforce.Models.Soql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Compos.Coreforce
{
    public interface ISalesforceRepository<T>
    {
        Task<List<T>> GetAsync(FilterItemCollection<T> filterCollection);
        Task<List<T>> GetAsync(params Expression<Func<T, object>>[] selectItems);
        Task<List<T>> GetAsync(FilterItemCollection<T> filterCollection, params Expression<Func<T, object>>[] selectItems);
        Task<T> GetByIdAsync(string id);
        Task<InsertResult> InsertAsync(T obj);
        Task<SObjectTreeResult> InsertAsync<U>(List<U> objList, bool stopByException = true, bool parallelProcessing = false)
            where U : InsertAttributes, T, new();
        Task<UpdateResult> UpdateAsync(T obj, params Expression<Func<T, object>>[] relatingObject);
        Task<BatchResult> UpdateAsync(List<T> objList, params Expression<Func<T, object>>[] relatingObject);
        Task<BatchResult> DeleteAsync(T obj);
        Task<BatchResult> DeleteAsync(List<T> objList);
        Task<List<T>> Query(string statement);
    }
}
