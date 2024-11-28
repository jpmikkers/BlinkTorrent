using MonoTorrent;
using MonoTorrent.Client;
using System.ComponentModel;

namespace QueueTorrent;

public class TorrentTracker : ITorrentTracker
{
    public Uri Uri { get; set; } = default!;

    public int Tier { get; set; } = 0;

    public bool Active { get; set; } = false;

    public bool CanScrape { get; set; }

    public TimeSpan MinUpdateInterval { get; set; }

    public TimeSpan UpdateInterval { get; set; }

    public TimeSpan TimeSinceLastAnnounce { get; set; }

    public string Status { get; set; } = string.Empty;

    public string WarningMessage { get; set; } = string.Empty;

    public string FailureMessage { get; set; } = string.Empty;
}
