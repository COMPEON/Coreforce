using Compos.Coreforce.Models;

namespace Compos.Coreforce
{
    /// <summary>
    /// Add sobject attributes attribute to salesforce class. This class has to be used when you want to use the Insert(List<T> objs) method of the repository.
    /// </summary>
    public class InsertAttributes
    {
        public SObjectTreeAttribute attributes { get; set; }
    }
}
