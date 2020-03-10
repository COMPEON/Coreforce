namespace Compos.Coreforce.Filter
{
    public interface IFilterCollection : IFilter
    {
        void Add(IFilter filter);
    }
}
