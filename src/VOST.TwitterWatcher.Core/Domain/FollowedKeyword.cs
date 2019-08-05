namespace VOST.TwitterWatcher.Repo
{
    public sealed class FollowedKeyword : Entity
    {
        public string Keyword { get; set; }

        public bool Enabled { get; set; } = true;
    }
}
