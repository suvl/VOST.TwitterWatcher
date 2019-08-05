using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VOST.TwitterWatcher.Repo;

namespace VOST.TwitterWatcher.Core.Interfaces
{
    public interface ITweetRepository : IRepository<TweetRecord>
    {
        Task<IList<TweetRecord>> GetRecordsPaged(int page, int pageSize, DateTime? before, DateTime? after);
    }
}