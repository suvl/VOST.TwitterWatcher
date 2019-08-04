namespace VOST.TwitterWatcher.Core.Configuration
{
    /// <summary>
    /// Mongo DB client configuration
    /// </summary>
    public class MongoDbClientConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; set; }

        public void ValidateAndThrow()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString)) throw new System.ArgumentNullException(nameof(ConnectionString));
        }
    }
}