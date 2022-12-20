using MonoTorrent;
using MonoTorrent.Client;
using System.ComponentModel;

namespace QueueTorrent
{
    public class TorrentTrackerItem : ITorrentTrackerItem
    {
        public int Tier { get; set; } = 0;

        public string Status { get; set; } = string.Empty;

        public Uri Uri { get; set; } = default!;

        public bool Active { get; set; } = false;
    }
}
