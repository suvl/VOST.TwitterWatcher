using System.Collections.Generic;

namespace VOST.TwitterWatcher.Core.DTO
{
    public struct KeywordStats
    {
        public int Count { get; set; }

        public IEnumerable<HistogramEntry> HistogramData { get; set; }

        public IEnumerable<WordEntry> WordCloud { get; set; }
    }
}