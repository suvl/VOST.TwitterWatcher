using System.Threading;
using System.Threading.Tasks;

namespace VOST.TwitterWatcher.Repo
{
    /// <summary>
    /// Interface for repository.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public interface IRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Inserts the specific entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// .
        /// </returns>
        Task InsertAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// .
        /// </returns>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets an entity by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// The entity.
        /// </returns>
        Task<T> GetAsync(object id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes an entity by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// The deleted entity.
        /// </returns>
        Task<T> DeleteByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// The deleted entity.
        /// </returns>
        Task<T> DeleteAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));
    }

}
