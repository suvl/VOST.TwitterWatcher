using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace VOST.TwitterWatcher.Repo
{
    /// <summary>
    /// Atomic update strategy for mongo. This strategy uses the traditional "conditional update" method.
    /// This strategy throws an EntityConflictException if the update fails.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public class AtomicUpdateStrategy<T> where T : Entity
    {
        /// <summary>
        /// Gets the atomic filter.
        /// </summary>
        /// <value>
        /// The atomic filter.
        /// </value>
        public Func<T, FilterDefinition<T>> AtomicFilter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AtomicUpdateStrategy class.
        /// </summary>
        /// <param name="atomicFilter">The atomic filter.</param>
        public AtomicUpdateStrategy(Func<T, FilterDefinition<T>> atomicFilter)
        {
            AtomicFilter = atomicFilter;
        }

        /// <summary>
        /// Executes the update asynchronous operation.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateOptions">(optional) options for controlling the update.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// .
        /// </returns>
        public async Task<ReplaceOneResult> ExecuteUpdateAsync(
            IMongoCollection<T> collection,
            T entity,
            UpdateOptions updateOptions = null,
            CancellationToken cancellationToken = default)
        {
            var baseFilter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
            var updateCondition = Builders<T>.Filter.And(baseFilter, AtomicFilter(entity));
            var result = await collection.ReplaceOneAsync(updateCondition, entity, updateOptions, cancellationToken);

            if (result != null && IsResultValid(result))
                throw new EntityConflictException(entity, "Update failed because entity versions conflict!");

            return result;
        }

        /// <summary>
        /// Validates the update result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        protected virtual bool IsResultValid(ReplaceOneResult result)
        {
            return result.IsAcknowledged && result.MatchedCount == 0 ||
                result.IsModifiedCountAvailable && result.ModifiedCount <= 0;
        }

    }

}