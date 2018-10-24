namespace Compos.Coreforce.Models.Soql
{
    public interface IFilterItem<T>
    {
        string BuildStatement();
    }
}
