namespace VOST.TwitterWatcher.Core.Configuration
{
    /// <summary>
    /// Twitter API configuration object.
    /// </summary>
    /// <remarks>
    /// Check https://developer.twitter.com/en/docs/basics/authentication/overview/application-only for further details.
    /// </remarks>
    public sealed class TwitterApiConfiguration
    {
        /// <summary>
        /// Gets or sets the consumer key.
        /// </summary>
        /// <value>
        /// The consumer key.
        /// </value>
        public string ConsumerKey { get; set; }

        /// <summary>
        /// Gets or sets the consumer secret.
        /// </summary>
        /// <value>
        /// The consumer secret.
        /// </value>
        public string ConsumerSecret { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the access token secret.
        /// </summary>
        /// <value>
        /// The access token secret.
        /// </value>
        public string AccessTokenSecret { get; set; }

        /// <summary>
        /// Gets or sets the followed keywords.
        /// </summary>
        /// <value>
        /// The followed keywords.
        /// </value>
        public string FollowedKeywords { get; set; }

        /// <summary>
        /// Validates the configuration and throws a <c>System.ArgumentNullException</c>
        /// if the configuration is invalid.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// ConsumerKey
        /// or
        /// ConsumerSecret
        /// or
        /// AccessToken
        /// or
        /// AccessTokenSecret
        /// </exception>
        /// 
        public void ValidateAndThrow()
        {
            if (string.IsNullOrWhiteSpace(ConsumerKey)) throw new System.ArgumentNullException(nameof(ConsumerKey));
            if (string.IsNullOrWhiteSpace(ConsumerSecret)) throw new System.ArgumentNullException(nameof(ConsumerSecret));
            if (string.IsNullOrWhiteSpace(AccessToken)) throw new System.ArgumentNullException(nameof(AccessToken));
            if (string.IsNullOrWhiteSpace(AccessTokenSecret)) throw new System.ArgumentNullException(nameof(AccessTokenSecret));
        }
    }
}
