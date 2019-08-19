using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VOST.TwitterWatcher.Core.Configuration;

using MongoDB.Driver;
using VOST.TwitterWatcher.Core.Interfaces;

namespace VOST.TwitterWatcher.Repo
{
    /// <summary>
    /// The Tweet record repository.
    /// </summary>
    /// <seealso cref="TweetRecord" />
    public class TweetRepository : MongoRepository<TweetRecord>, ITweetRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TweetRepository"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public TweetRepository(IOptions<MongoDbClientConfiguration> configuration, ILogger<TweetRepository> logger)
            : base(configuration, logger)
        {
        }

        /// <summary>
        /// Gets the records with pagination.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="before">The before.</param>
        /// <param name="after">The after.</param>
        /// <returns>
        /// The record's list
        /// </returns>
        public async Task<IList<TweetRecord>> GetRecordsPaged(int page, int pageSize, System.DateTime? before, System.DateTime? after)
        {
            var filters = new List<FilterDefinition<TweetRecord>>(2);

            if (before.HasValue)
            {
                filters.Add(Filter.Gte(r => r.Updated, before.Value.ToUniversalTime()));
            }

            if (after.HasValue)
            {
                filters.Add(Filter.Lte(r => r.Updated, after.Value.ToUniversalTime()));
            }

            var filter = filters.Count == 0
                ? Filter.Empty
                : Filter.And(filters);

            var find = Collection.Find(filter, new FindOptions { BatchSize = pageSize })
                .SortByDescending(r => r.Updated)
                .Skip(page * pageSize)
                .Limit(pageSize);

            return await find.ToListAsync();
        }

        /// <summary>
        /// Gets all records that match a keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>The collection of tweet records.</returns>
        /// <exception cref="ArgumentNullException">keyword</exception>
        public async Task<ICollection<TweetRecord>> GetAllRecordsByKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) throw new ArgumentNullException(nameof(keyword));

            return await Collection
                .Find(Filter.All(r => r.MatchedKeywords, new []{keyword}))
                .ToListAsync();
        }
    }
}
