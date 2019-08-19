using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VOST.TwitterWatcher.Repo;

namespace VOST.TwitterWatcher.Core.Interfaces
{
    public interface ITweetRepository : IRepository<TweetRecord>
    {
        Task<IList<TweetRecord>> GetRecordsPaged(int page, int pageSize, DateTime? before, DateTime? after);

        /// <summary>
        /// Gets all records that match a keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>The collection of tweet records.</returns>
        /// <exception cref="ArgumentNullException">keyword</exception>
        Task<ICollection<TweetRecord>> GetAllRecordsByKeyword(string keyword);
    }
}