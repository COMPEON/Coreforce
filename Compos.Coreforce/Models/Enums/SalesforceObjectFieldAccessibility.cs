namespace Compos.Coreforce
{
    internal enum SalesforceObjectFieldAccessibility
    {
        /// <summary>
        /// Return all sObject fields.
        /// </summary>
        All,
        /// <summary>
        /// Return all fields that can be set by insert.
        /// </summary>
        Insert,
        /// <summary>
        /// Return all fields that can be set by update.
        /// </summary>
        Update
    }
}
