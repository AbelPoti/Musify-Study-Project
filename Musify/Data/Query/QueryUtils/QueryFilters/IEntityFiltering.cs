namespace Musify.Data.Query.QueryUtils.QueryFilters
{
    public interface IEntityFiltering<TEntity, in TFilter>
    {
        IQueryable<TEntity> Apply(IQueryable<TEntity> query, TFilter filter);
    }
}