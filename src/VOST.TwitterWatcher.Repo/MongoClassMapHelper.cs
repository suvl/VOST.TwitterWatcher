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
                if (!BsonClassMap.IsClassMapRegistered(typeof(TweetRecord)))
                    BsonClassMap.RegisterClassMap<TweetRecord>();
                if (!BsonClassMap.IsClassMapRegistered(typeof(FollowedKeyword)))
                    BsonClassMap.RegisterClassMap<FollowedKeyword>();
            }
        }
    }
}