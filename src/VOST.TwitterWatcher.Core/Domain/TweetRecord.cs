namespace VOST.TwitterWatcher.Repo
{
    /// <summary>
    /// A Tweet recorded from the API
    /// </summary>
    /// <seealso cref="LinqToTwitter.Status" />
    /// <seealso cref="VOST.TwitterWatcher.Repo.IEntity" />
    public sealed class TweetRecord : Entity
    {
        public LinqToTwitter.Status Status { get; set; }
        
        public TweetRecord()
        {
            Id = System.Guid.NewGuid().ToString("N");
            Updated = Created = System.DateTime.UtcNow;
        }
    }
}
