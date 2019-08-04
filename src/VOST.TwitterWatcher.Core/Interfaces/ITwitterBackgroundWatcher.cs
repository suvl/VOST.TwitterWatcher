using Microsoft.Extensions.Hosting;

namespace VOST.TwitterWatcher.Core.Interfaces
{
    public interface ITwitterBackgroundWatcher : IHostedService
    {

        System.Threading.Tasks.Task Subscribe();
    }
}
