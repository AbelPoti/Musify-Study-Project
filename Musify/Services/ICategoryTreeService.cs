using Musify.Models;

namespace Musify.Services
{
    public interface ICategoryTreeService
    {
        /// <summary>
        ///     Get the descendant <see cref="Category"/> Ids of a category represented by
        ///     <paramref name="rootCategoryId"/>, including <paramref name="rootCategoryId"/>.
        /// </summary>
        /// <param name="rootCategoryId">The root of the category tree to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The Ids from the flattened category tree."/></returns>
        Task<IReadOnlySet<int>> GetDescendantIdsAsync(
            int rootCategoryId,
            CancellationToken cancellationToken);
        
        /// <summary>
        ///     Get the descendant <see cref="Category"/> entities of a category represented by
        ///     <paramref name="rootCategoryId"/>, including the <see cref="Category"/> with
        ///     <paramref name="rootCategoryId"/>.
        /// </summary>
        /// <param name="rootCategoryId">The root of the category tree to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The flattened category tree.</returns>
        Task<IEnumerable<Category>> GetDescendantCategoriesAsync(
            int rootCategoryId,
            CancellationToken cancellationToken);
        
        Task<IEnumerable<Category>> GetAncestorCategoriesAsync(
            int rootCategoryId,
            CancellationToken cancellationToken);
        
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync(CancellationToken cancellationToken);
    }
}