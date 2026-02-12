namespace Musify.Services
{
    public interface ICategoryTreeService
    {
        Task<IReadOnlySet<int>> GetDescendantIdsAsync(
            int rootCategoryId,
            CancellationToken cancellationToken);
    }
}