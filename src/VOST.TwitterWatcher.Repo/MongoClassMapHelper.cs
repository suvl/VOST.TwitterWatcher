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
                if (!BsonClassMap.IsClassMapRegistered(typeof(Entity)))
                    BsonClassMap.RegisterClassMap<Entity>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapIdMember(e => e.Id);
                    });
                if (!BsonClassMap.IsClassMapRegistered(typeof(LinqToTwitter.EntityBase)))
                    BsonClassMap.RegisterClassMap<LinqToTwitter.EntityBase>(cm =>
                    {
                        cm.AutoMap();
                        cm.SetIsRootClass(true);
                    });
                if (!BsonClassMap.IsClassMapRegistered(typeof(LinqToTwitter.MediaEntity)))
                    BsonClassMap.RegisterClassMap<LinqToTwitter.MediaEntity>(cm =>
                    {
                        cm.AutoMap();
                        cm.UnmapMember(e => e.Indices);
                    });
                if (!BsonClassMap.IsClassMapRegistered(typeof(LinqToTwitter.UrlEntity)))
                    BsonClassMap.RegisterClassMap<LinqToTwitter.UrlEntity>(cm =>
                    {
                        cm.AutoMap();
                        cm.UnmapMember(e => e.Indices);
                    });
                if (!BsonClassMap.IsClassMapRegistered(typeof(TweetRecord)))
                    BsonClassMap.RegisterClassMap<TweetRecord>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapMember(e => e.Status);
                    });
                if (!BsonClassMap.IsClassMapRegistered(typeof(FollowedKeyword)))
                    BsonClassMap.RegisterClassMap<FollowedKeyword>();
            }
        }
    }
}