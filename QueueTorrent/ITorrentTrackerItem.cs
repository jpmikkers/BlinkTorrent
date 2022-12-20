using MonoTorrent;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace QueueTorrent
{
    public interface ITorrentTrackerItem
    {
        public int Tier { get; }
        public bool Active { get; }
        public Uri Uri { get; }
        public string Status { get; }
    }
}