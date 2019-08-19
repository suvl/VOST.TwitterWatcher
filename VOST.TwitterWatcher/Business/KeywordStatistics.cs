using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tweetinvi.Core.Extensions;
using VOST.TwitterWatcher.Core.DTO;
using VOST.TwitterWatcher.Repo;

namespace VOST.TwitterWatcher.Business
{
    internal static class KeywordStatistics
    {
        private static readonly Regex WordRegex = new Regex(@"\w+", RegexOptions.Compiled);

        public static KeywordStats CreateStats(ICollection<TweetRecord> records)
        {
            if (records.Count == 0)
                return new KeywordStats {Count = 0};

            // histogram data
            var histogramData = records
                .OrderBy(r => r.CreatedAt)
                .Select(r => new HistogramEntry
                {
                    CreatedAt = r.CreatedAt,
                    Text = r.Text
                });

            // word cloud
            var wordCloud = new Dictionary<string, int>();
            var entireText = records
                .Aggregate(
                    new StringBuilder(),
                    (builder, record) => builder.AppendJoin(' ',
                        string.IsNullOrEmpty(record.FullText) ? record.Text : record.FullText));
            WordRegex.Matches(entireText.ToString())
                .ForEach(m =>
                {
                    if (wordCloud.ContainsKey(m.Value))
                        wordCloud[m.Value]++;
                    else wordCloud[m.Value] = 1;
                });

            return new KeywordStats
            {
                Count = records.Count,
                HistogramData = histogramData,
                WordCloud = wordCloud
                    .OrderByDescending(r => r.Value)
                    .Take(100)
                    .Select(r => new WordEntry
                    {
                        Count = r.Value,
                        Word = r.Key
                    })
            };
        }
    }
}
