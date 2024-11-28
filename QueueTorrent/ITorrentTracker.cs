using MonoTorrent;
using MonoTorrent.Trackers;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace QueueTorrent;

public interface ITorrentTracker
{
    int Tier { get; }
    Uri Uri { get; }
    bool Active { get; }
    bool CanScrape { get; }
    TimeSpan MinUpdateInterval { get; }
    TimeSpan UpdateInterval { get; }
    TimeSpan TimeSinceLastAnnounce { get; }
    string Status { get; }
    string WarningMessage { get; }
    string FailureMessage { get; }
}