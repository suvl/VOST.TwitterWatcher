using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VOST.TwitterWatcher.Repo
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="NOS.Repository.IRepository{TEntity}" />
    public abstract class MongoRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        /// <summary>
        /// The collection
        /// </summary>
        protected readonly IMongoCollection<TEntity> Collection;

        /// <summary>
        /// The filter builder
        /// </summary>
        protected readonly FilterDefinitionBuilder<TEntity> Filter = new FilterDefinitionBuilder<TEntity>();

        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">connectionString</exception>
        /// <exception cref="ArgumentNullException">logger</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// configuration - Connection string in configuration must not be null or empty
        /// </exception>
        protected MongoRepository(IOptions<Core.Configuration.MongoDbClientConfiguration> configuration, ILogger logger)
            : this(configuration, logger, typeof(TEntity).Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <exception cref="ArgumentNullException">
        /// configuration
        /// or
        /// logger
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">configuration - Connection string in configuration must not be null or empty</exception>
        protected MongoRepository(
            IOptions<Core.Configuration.MongoDbClientConfiguration> configuration,
            ILogger logger,
            string collectionName)
        {
            if (configuration?.Value == null) throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            MongoClassMapHelper.SetupClassMap();

            var connectionString = configuration.Value.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentOutOfRangeException(
                    nameof(configuration),
                    "Connection string in configuration must not be null or empty");

            var url = new MongoUrl(connectionString);

            IMongoClient client = new MongoClient(url);
            var database = client.GetDatabase(url.DatabaseName);
            Collection = database.GetCollection<TEntity>(collectionName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{TEntity}"/> class.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        protected MongoRepository(string connectionString, ILogger logger) : this(connectionString, logger, typeof(TEntity).Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <exception cref="ArgumentOutOfRangeException">connectionString - Connection string in configuration must not be null or empty</exception>
        /// <exception cref="ArgumentNullException">logger</exception>
        protected MongoRepository(string connectionString, ILogger logger, string collectionName)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentOutOfRangeException(
                    nameof(connectionString),
                    "Connection string in configuration must not be null or empty");

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            MongoClassMapHelper.SetupClassMap();
            var url = new MongoUrl(connectionString);

            IMongoClient client = new MongoClient(url);
            var database = client.GetDatabase(url.DatabaseName);
            Collection = database.GetCollection<TEntity>(collectionName);
        }

        private readonly InsertOneOptions _insertOneOptions = new InsertOneOptions();
        private readonly UpdateOptions _updateOptions = new UpdateOptions() { IsUpsert = true };
        private readonly FindOneAndDeleteOptions<TEntity> _deleteOptions = new FindOneAndDeleteOptions<TEntity>();

        private readonly AggregateOptions _aggregateOptions =
            new AggregateOptions { AllowDiskUse = true, UseCursor = true };

        /// <inheritdoc />
        /// <summary>
        /// Inserts the specific entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// .
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">entity</exception>
        public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id)) throw new ArgumentNullException(nameof(entity));
            Logger.LogDebug("InsertAsync {0}", entity);
            entity.Created = DateTime.UtcNow;
            entity.Updated = DateTime.UtcNow;
            try
            {
                await Collection.InsertOneAsync(entity, _insertOneOptions, cancellationToken);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    Logger.LogError(ex, "Write exception while inserting entity due to duplicated Key.");
                    throw;
                }

                Logger.LogError(ex, "Write exception while inserting entity");
                throw;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// .
        /// </returns>
        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id))
                throw new ArgumentNullException(nameof(entity));
            await ExecuteUpdateAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Executes the update operation.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// .
        /// </returns>
        private async Task ExecuteUpdateAsync(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            Logger.LogDebug("ExecuteUpdateAsync {0}", entity);

            var prevUpdated = entity.Updated;
            var prevVersion = entity.Version;

            try
            {
                entity.Version++;
                entity.Updated = DateTime.UtcNow;
                var filter = Filter.Eq(_ => _.Id, entity.Id);
                var updateCondition = Filter.Lt(_ => _.Version, entity.Version);

                ReplaceOneResult result = await Collection.ReplaceOneAsync(Filter.And(updateCondition, filter), entity, _updateOptions, cancellationToken);
                if (!WasEntityCreated(result))
                {
                    if (IsEntityNotFound(result))
                        throw new EntityNotFoundException(entity);
                    if (IsEntityNotChanged(result))
                        throw new EntityConflictException(entity);
                }
            }
            catch (Exception)
            {
                entity.Updated = prevUpdated;
                entity.Version = prevVersion;
                throw;
            }
        }

        private bool WasEntityCreated(ReplaceOneResult result)
        {
            return result != null
                   && result.IsAcknowledged
                   && result.MatchedCount == 0
                   && result.IsModifiedCountAvailable
                   && result.ModifiedCount == 0
                   && result.UpsertedId != null;
        }

        private bool IsEntityNotFound(ReplaceOneResult result)
        {
            return result != null && result.IsAcknowledged && result.MatchedCount == 0;
        }

        private bool IsEntityNotChanged(ReplaceOneResult result)
        {
            return result != null && result.IsAcknowledged && result.IsModifiedCountAvailable && result.ModifiedCount <= 0;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets an entity by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// The entity.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">id</exception>
        public virtual async Task<TEntity> GetAsync(object id, CancellationToken cancellationToken = new CancellationToken())
        {
            var idStr = id?.ToString() ?? throw new ArgumentNullException(nameof(id));
            Logger.LogDebug("GetAsync {id}", id);
            var filter = Filter.Eq(_ => _.Id, idStr);
            return await (await Collection.FindAsync<TEntity>(filter)).SingleOrDefaultAsync();
        }

        /// <inheritdoc />
        /// <summary>Deletes an entity by id.</summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>The deleted entity.</returns>
        public async Task<TEntity> DeleteByIdAsync(object id, CancellationToken cancellationToken = new CancellationToken())
        {
            var idStr = id?.ToString() ?? throw new ArgumentNullException(nameof(id));
            Logger.LogDebug("DeleteByIdAsync {id}", id);
            var filter = Filter.Eq(_ => _.Id, idStr);
            return await Collection.FindOneAndDeleteAsync(filter, _deleteOptions, cancellationToken);
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">(optional) the cancellation token.</param>
        /// <returns>
        /// The deleted entity.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">entity</exception>
        public async Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id)) throw new ArgumentNullException(nameof(entity));
            Logger.LogDebug("DeleteAsync {0}", entity);
            return await DeleteByIdAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        ///     Creates a query for this repository.
        /// </summary>
        /// <returns>The query for the repository.</returns>
        public IQueryable<TEntity> AsQueryable()
        {
            return Collection.AsQueryable(_aggregateOptions);
        }
    }
}