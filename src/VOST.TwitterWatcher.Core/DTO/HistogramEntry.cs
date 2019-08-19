using System;

namespace VOST.TwitterWatcher.Core.DTO
{
    public struct HistogramEntry
    {
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}