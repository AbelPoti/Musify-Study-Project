using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Models;

namespace Musify.Services
{
    public class CategoryTreeService : ICategoryTreeService
    {
        private readonly MusifyDbContext _dbContext;
        
        public CategoryTreeService(MusifyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        /// <summary>
        ///     Get the descendant <see cref="Category"/> Ids of a category represented by
        ///     <paramref name="rootCategoryId"/>, including <paramref name="rootCategoryId"/>.
        /// </summary>
        /// <param name="rootCategoryId">The root of the category tree to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns></returns>
        public async Task<IReadOnlySet<int>> GetDescendantIdsAsync(
            int rootCategoryId,
            CancellationToken cancellationToken)
        {
            // Perform a DFS for children of categories
            var lookup = await GetLookupForAllCategoriesAsync(cancellationToken);
            var result = new HashSet<int> { rootCategoryId };
            var stack = new Stack<int>();
            stack.Push(rootCategoryId);
            
            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (lookup.TryGetValue(current, out var children))
                {
                    foreach (var child in children)
                    {
                        if (result.Add(child))
                        {
                            stack.Push(child);
                        }
                    }
                }
            }

            return result;
        }
        
        private async Task<Dictionary<int, IEnumerable<int>>> GetLookupForAllCategoriesAsync(
            CancellationToken cancellationToken)
        {
            List<Category> categories = await GetAllCategoryIdsAsync(cancellationToken);
            // Fine to leave out root categories, since the tree will be looked up through ParentCategoryIds, so
            // if a root category has any children, it will be included in the lookup dictionary as a key
            var childrenByParent = categories
                .Where(c => c.ParentId != null)
                .GroupBy(c => c.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id));
            return childrenByParent;
        }
        
        private Task<List<Category>> GetAllCategoryIdsAsync(CancellationToken cancellationToken)
        {
            return _dbContext.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}