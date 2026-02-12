using Musify.Models;

namespace Musify.Services
{
    public interface ICategoryTreeService
    {
        Task<IReadOnlySet<int>> GetDescendantIdsAsync(
            int rootCategoryId,
            CancellationToken cancellationToken);
        
        Task<IEnumerable<Category>> GetDescendantCategoriesAsync(
            int rootCategoryId,
            CancellationToken cancellationToken);
    }
}