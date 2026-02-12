namespace Musify.Data.Query.QueryUtils.QueryFilters
{
    public interface IEntityFiltering<TEntity, in TFilter>
    {
        Task<IQueryable<TEntity>> Apply(IQueryable<TEntity> query, TFilter filter, CancellationToken cT);
    }
}