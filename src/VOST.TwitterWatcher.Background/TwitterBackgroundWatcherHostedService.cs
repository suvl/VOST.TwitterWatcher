using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using VOST.TwitterWatcher.Core.Interfaces;

namespace VOST.TwitterWatcher.Background
{
    public class TwitterBackgroundWatcherHostedService : IHostedService
    {
        private readonly ITwitterBackgroundWatcher _watcher;

        public TwitterBackgroundWatcherHostedService(ITwitterBackgroundWatcher watcher)
        {
            _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _watcher.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _watcher.StopAsync(cancellationToken);
        }
    }
}