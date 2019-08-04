using AutoMapper;
using System;

namespace VOST.TwitterWatcher.Core.Mapping
{
    /// <summary>
    /// The automapping util.
    /// </summary>
    public sealed class Mapper
    {
        private static Lazy<IMapper> StaticMapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<LinqToTwitter.Status, Repo.TweetRecord>();
            });
            return config.CreateMapper();
        });

        /// <summary>
        /// Gets the static Mapper instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IMapper Instance => StaticMapper.Value;
    }
}
