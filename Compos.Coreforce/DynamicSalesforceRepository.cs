using Compos.Coreforce.Models.Soql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Compos.Coreforce
{
    public class DynamicSalesforceRepository : IDynamicSalesforceRepository
    {
        public Task<List<dynamic>> GetAsync(string sObject)
        {
            throw new NotImplementedException();
        }

        public Task<List<dynamic>> GetAsync(string sObject, params string[] fields)
        {
            throw new NotImplementedException();
        }

        public Task<List<dynamic>> GetAsync(string sObject, FilterItemCollection<dynamic> items, params string[] fields)
        {
            throw new NotImplementedException();
        }

        public Task<dynamic> GetByIdAsync(string sObject, string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<dynamic>> Query(string query)
        {
            throw new NotImplementedException();
        }
    }
}
