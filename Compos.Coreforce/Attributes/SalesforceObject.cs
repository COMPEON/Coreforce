using System;

namespace Compos.Coreforce.Attributes
{
    public class SalesforceObject : Attribute
    {
        public string Name { get; private set; }

        /// <summary>
        /// Mark class as sObject. 
        /// </summary>
        /// <param name="name">Name of the sObject.</param>
        public SalesforceObject(string name)
        {
            Name = name;
        }
    }
}
