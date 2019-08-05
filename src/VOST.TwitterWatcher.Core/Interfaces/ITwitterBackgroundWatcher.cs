using System.Threading;
using System.Threading.Tasks;

namespace VOST.TwitterWatcher.Core.Interfaces
{
    public interface ITwitterBackgroundWatcher
    {

        /// <summary>
        /// Starts the subscription.
        /// </summary>
        Task Subscribe();

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
