using System;
using System.Threading;
using System.Threading.Tasks;
using LinqToTwitter;
using System.Linq;
using Microsoft.Extensions.Logging;
using Polly;
using Microsoft.Extensions.Options;
using VOST.TwitterWatcher.Repo;
using System.Collections.Generic;
using VOST.TwitterWatcher.Core.Mapping;

namespace VOST.TwitterWatcher.Background
{
    /// <summary>
    ///   <para>A Twitter Background Watcher, for ingesting tweeter events towards a destination.</para>
    /// </summary>
    /// <seealso cref="VOST.TwitterWatcher.Core.Interfaces.ITwitterBackgroundWatcher" />
    /// <seealso cref="System.IDisposable" />
    public sealed class TwitterBackgroundWatcher : Core.Interfaces.ITwitterBackgroundWatcher, IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IAuthorizer _authorizer;
        private readonly ILogger _logger;

        private readonly object _lock = new object();
        private TwitterContext _context;
        private TwitterContext Context
        {
            get
            {
                if (_context == null)
                {
                    lock (_lock)
                    {
                        if (_context == null)
                        {
                            _context = new TwitterContext(_authorizer);
                        }
                    }
                }
                return _context;
            }
        }

        private volatile IDisposable _currentSubscription = null;

        private readonly IRepository<TweetRecord> _repository;

        private readonly string _keywords;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterBackgroundWatcher"/> class.
        /// </summary>
        /// <param name="key">The Twitter API key.</param>
        /// <param name="secret">The Twitter API secret.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// twitterApiConfiguration.ValidateAndThrow()
        /// or
        /// logger
        /// </exception>
        public TwitterBackgroundWatcher(
            IOptions<Core.Configuration.TwitterApiConfiguration> twitterApiConfiguration,
            IRepository<Repo.TweetRecord> repository,
            ILogger<TwitterBackgroundWatcher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            if (twitterApiConfiguration.Value == null) throw new ArgumentNullException(nameof(twitterApiConfiguration));

            var config = twitterApiConfiguration.Value;
            config.ValidateAndThrow();

            _authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = config.ConsumerKey,
                    ConsumerSecret = config.ConsumerSecret,
                    AccessToken = config.AccessToken,
                    AccessTokenSecret = config.AccessTokenSecret
                }
            };

            _keywords = config.FollowedKeywords;
            
            _logger.LogInformation("{0} .ctor", nameof(TwitterBackgroundWatcher));
        }

        /// <summary>
        /// Starts the background Twitter watcher.
        /// </summary>
        /// <param name="cancellationToken">The initialization cancellation token.</param>
        /// <returns>The initialization Task.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Task.CompletedTask;

            _logger.LogInformation("{0}:{1}", nameof(TwitterBackgroundWatcher), nameof(StartAsync));

            return Subscribe();
        }

        public Task Subscribe()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(retry =>
                {
                    var waitTime = TimeSpan.FromMilliseconds(Math.Pow(50.0, retry));
                    return waitTime.TotalMinutes > 5.0 ? TimeSpan.FromMinutes(5.0) : waitTime;
                }, 
                (ex,ts) => _logger.LogError(ex, "Error on RefreshSubscription, waiting {0} to retry", ts))
                .ExecuteAsync(RefreshSubscription);
        }

        private async Task RefreshSubscription()
        {
            _currentSubscription?.Dispose();

            if (_cts.IsCancellationRequested) return;

            var observable = await Context.Streaming
                .WithCancellation(_cts.Token)
                .Where(streaming =>
                    streaming.Type == StreamingType.Filter &&
                    streaming.Track == _keywords)
                .ToObservableAsync();

            _logger.LogDebug("Subscribing to stream data");

            _currentSubscription = observable.Subscribe(
                async stream => await HandleStream(stream),
                ex =>
                {
                    _logger.LogError(ex, "Subscription error.");
                    throw ex;
                },
                () => _logger.LogInformation("Stream subscription completed"));
        }

        /// <summary>
        /// Stops the background tasks.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stopping async task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _currentSubscription?.Dispose();
            _cts.Cancel(true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the stream result coming from twitter.
        /// </summary>
        /// <param name="streamContent">Content of the stream.</param>
        /// <returns>The async processing task.</returns>
        private Task HandleStream(StreamContent streamContent)
        {
            if (streamContent.EntityType == StreamEntityType.Status)
            {
                return HandleStatus(streamContent.Entity as Status);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the twitter status update.
        /// </summary>
        /// <param name="entity">The status update's content.</param>
        /// <returns>The processing task.</returns>
        private async Task HandleStatus(Status entity)
        {
            // save this thing at the TSDB
            _logger.LogInformation("Received tweet | {0}", entity.Text);

            var record = new TweetRecord
            {
                Status = entity
            };

            try
            {
                await _repository.InsertAsync(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "unable to save tweet status to db.");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
            _cts?.Dispose();
        }
    }
}
