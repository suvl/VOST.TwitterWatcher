using MongoDB.Bson.Serialization;

namespace VOST.TwitterWatcher.Repo
{
    internal static class MongoClassMapHelper
    {
        static MongoClassMapHelper()
        {
        }

        /// <summary>
        /// locker object
        /// </summary>
        private static object locker = new object();

        public static void SetupClassMap()
        {
            lock (locker)
            {
                BsonClassMap.RegisterClassMap<Entity>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(e => e.Id);
                });
                BsonClassMap.RegisterClassMap<LinqToTwitter.EntityBase>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIsRootClass(true);
                });
                BsonClassMap.RegisterClassMap<LinqToTwitter.MediaEntity>(cm =>
                {
                    cm.AutoMap();
                    cm.UnmapMember(e => e.Indices);
                });
                BsonClassMap.RegisterClassMap<LinqToTwitter.UrlEntity>(cm =>
                {
                    cm.AutoMap();
                    cm.UnmapMember(e => e.Indices);
                });
                BsonClassMap.RegisterClassMap<TweetRecord>(cm => 
                {
                    cm.AutoMap();
                    cm.MapMember(e => e.Status);
                });
            }
        }
    }
}