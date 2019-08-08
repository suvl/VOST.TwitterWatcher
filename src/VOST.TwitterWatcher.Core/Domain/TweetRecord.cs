using System;

namespace VOST.TwitterWatcher.Repo
{
    /// <summary>
    /// A Tweet recorded from the API
    /// </summary>
    /// <seealso cref="LinqToTwitter.Status" />
    /// <seealso cref="VOST.TwitterWatcher.Repo.IEntity" />
    public sealed class TweetRecord : Entity
    {
        public string[] MatchedKeywords { get; set; }

        public string TweetJson { get; set; }

        public string Text { get; set; }

        public string FullText { get; set; }

        public string Location { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string User { get; set; }

        public long UserId { get; set; }

        public string UserHandle { get; set; }

        public string UserPictureUrl { get; set; }

        public string UserProfileUrl { get; set; }

        public string UserLocation { get; set; }

        public bool IsRetweet { get; set; }

        public int RetweetCount { get; set; }

        public int FavoriteCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Truncated { get; set; }
        
        public TweetRecord()
        {
            Id = System.Guid.NewGuid().ToString("N");
            Updated = Created = System.DateTime.UtcNow;
        }
    }
}
