using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Polly;
using Microsoft.Extensions.Options;
using VOST.TwitterWatcher.Repo;

using Tweetinvi;
using Tweetinvi.Models;

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
        private readonly ILogger _logger;

        private readonly ITwitterCredentials _credentials;

        private volatile Tweetinvi.Streaming.IFilteredStream _stream;

        private readonly Core.Interfaces.ITweetRepository _repository;
        private readonly Core.Interfaces.IKeywordRepository _keywordRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterBackgroundWatcher" /> class.
        /// </summary>
        /// <param name="twitterApiConfiguration">The twitter API configuration.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="keywordRepository">The keyword repository.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">twitterApiConfiguration.ValidateAndThrow()
        /// or
        /// logger</exception>
        public TwitterBackgroundWatcher(
            IOptions<Core.Configuration.TwitterApiConfiguration> twitterApiConfiguration,
            Core.Interfaces.ITweetRepository repository,
            Core.Interfaces.IKeywordRepository keywordRepository,
            ILogger<TwitterBackgroundWatcher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _keywordRepository = keywordRepository ?? throw new ArgumentNullException(nameof(keywordRepository));

            if (twitterApiConfiguration.Value == null) throw new ArgumentNullException(nameof(twitterApiConfiguration));

            var config = twitterApiConfiguration.Value;
            config.ValidateAndThrow();

            _credentials = new TwitterCredentials(
                consumerKey: config.ConsumerKey, 
                consumerSecret: config.ConsumerSecret,
                accessToken: config.AccessToken,
                accessTokenSecret: config.AccessTokenSecret);

            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            
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

        /// <summary>
        /// Starts the subscription.
        /// </summary>
        /// <returns>The subscription's Task.</returns>
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
                .ExecuteAsync(NewRefreshSubscription);
        }

        private async Task NewRefreshSubscription()
        {
            if (_cts.IsCancellationRequested) return;

            var keywords = await _keywordRepository.GetFollowedKeywords();

            if (keywords.Count == 0) return;

            _stream?.StopStream();

            _stream = Stream.CreateFilteredStream(_credentials, TweetMode.Extended);

            foreach (var kw in keywords)
                _stream.AddTrack(kw.Keyword);

            _stream.MatchingTweetReceived += (sender, args) => {
                _logger.LogInformation("New Matching Tweet [Matched={0}, Tweet={1}]", args.MatchingTracks, args.Json);

                var tweet = args.Tweet;

                var matched = Enumerable.Empty<string>()
                    .Union(args.MatchingTracks)
                    .Union(args.QuotedTweetMatchingTracks)
                    .ToArray();

                var tweetRecord = new TweetRecord 
                {
                    MatchedKeywords = matched,
                    TweetJson = args.Json,
                    CreatedAt = tweet.CreatedAt,
                    FavoriteCount = tweet.FavoriteCount,
                    FullText = tweet.FullText,
                    Id = tweet.IdStr,
                    IsRetweet = tweet.IsRetweet,
                    RetweetCount = tweet.RetweetCount,
                    Text = tweet.Text,
                    Truncated = tweet.Truncated,
                    Location = tweet.Place?.FullName
                };

                if (tweet.CreatedBy != null)
                {
                    tweetRecord.User = tweet.CreatedBy.Name;
                    tweetRecord.UserId = tweet.CreatedBy.Id;
                    tweetRecord.UserLocation = tweet.CreatedBy.Location;
                    tweetRecord.UserPictureUrl = tweet.CreatedBy.ProfileImageUrl400x400;
                    tweetRecord.UserProfileUrl = tweet.CreatedBy.Url;
                    tweetRecord.UserHandle = tweet.CreatedBy.ScreenName;
                }

                if (tweet.Coordinates != null)
                {
                    tweetRecord.Latitude = tweet.Coordinates.Latitude;
                    tweetRecord.Longitude = tweet.Coordinates.Longitude;
                }

                ThreadPool.QueueUserWorkItem(_ => _repository.InsertAsync(tweetRecord, _cts.Token));
            };
            _stream.DisconnectMessageReceived += (s, args) => {
                _logger.LogError("DisconnectMessageReceived={0}", args.DisconnectMessage);
                // on disconnect, wait two minutes and connect again.
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(2));
                    await Subscribe();
                });
            };
            _stream.StreamStarted += (s, args) => {
                _logger.LogInformation("Stream started.");
            };
            _stream.WarningFallingBehindDetected += (e, args) => {
                _logger.LogWarning(
                    "WarningFallingBehindDetected [{0} {1} {2}%]", 
                    args.WarningMessage.Code,
                    args.WarningMessage.Message,
                    args.WarningMessage.PercentFull);
            };
            _stream.KeepAliveReceived += (e, args) => {
                _logger.LogInformation("KeepAlive");
            };
            _stream.LimitReached += (e, args) => {
                _logger.LogWarning("Limit reached (tweets not received={0})", args.NumberOfTweetsNotReceived);
            };
            
            // the next awaitable does not return, so we have to make it go to a new thread
            ThreadPool.QueueUserWorkItem(async _ => await _stream.StartStreamMatchingAnyConditionAsync());
        }

        /// <summary>
        /// Stops the background tasks.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stopping async task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stream?.StopStream();
            _cts.Cancel(true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _cts?.Dispose();
        }
    }
}
