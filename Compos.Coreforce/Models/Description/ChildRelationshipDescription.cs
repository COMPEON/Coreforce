using System.Collections.Generic;

namespace Compos.Coreforce.Models.Description
{
    public class ChildRelationshipDescription
    {
        public bool cascadeDelete { get; set; }
        public string childSObject { get; set; }
        public bool deprecatedAndHidden { get; set; }
        public string field { get; set; }
        public List<object> junctionIdListNames { get; set; }
        public List<object> junctionReferenceTo { get; set; }
        public string relationshipName { get; set; }
        public bool restrictedDelete { get; set; }
    }
}
