using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VOST.TwitterWatcher.Core.Configuration;
using VOST.TwitterWatcher.Core.Interfaces;

namespace VOST.TwitterWatcher.Repo
{
    public sealed class KeywordRepository : MongoRepository<FollowedKeyword>, IKeywordRepository
    {
        public KeywordRepository(IOptions<MongoDbClientConfiguration> configuration, ILogger<KeywordRepository> logger)
            : base(configuration, logger)
        {
        }

        public Task<IList<FollowedKeyword>> GetAllFollowedKeywords()
        {
            return GetKeywordsFiltered(Filter.Empty);
        }

        public Task<IList<FollowedKeyword>> GetFollowedKeywords()
        {
            return GetKeywordsFiltered(Filter.Eq(k => k.Enabled, true));
        }

        private async Task<IList<FollowedKeyword>> GetKeywordsFiltered(FilterDefinition<FollowedKeyword> filter)
        {
            var list = await Collection.Find(filter).ToListAsync();
            return list;
        }
    }
}
