using System.Collections.Generic;
using System.Threading.Tasks;
using VOST.TwitterWatcher.Repo;

namespace VOST.TwitterWatcher.Core.Interfaces
{
    public interface IKeywordRepository : IRepository<FollowedKeyword>
    {
        Task<IList<FollowedKeyword>> GetFollowedKeywords();
        Task<IList<FollowedKeyword>> GetAllFollowedKeywords();
    }
}