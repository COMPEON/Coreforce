using Compos.Coreforce.Models.Soql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Compos.Coreforce
{
    public interface IDynamicSalesforceRepository
    {
        Task<List<dynamic>> GetAsync(string sObject);
        Task<List<dynamic>> GetAsync(string sObject, params string[] fields);
        Task<List<dynamic>> GetAsync(string sObject, FilterItemCollection<dynamic> items, params string[] fields);
        Task<dynamic> GetByIdAsync(string sObject, string id);
        Task<List<dynamic>> Query(string query);
    }
}
