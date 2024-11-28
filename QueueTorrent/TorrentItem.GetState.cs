using MonoTorrent.Client;
using System;
using System.Diagnostics;

namespace QueueTorrent;

public partial class TorrentItem
{
    private TorrentItemState GetState()
    {
        var state = TorrentItemState.Queued;
        // Trace.WriteLine($"BT GetState(): {Name} {_manager.State}");

        switch(_manager.State)
        {
            case TorrentState.Starting:
                state = TorrentItemState.Starting;
                break;

            case TorrentState.Downloading:
                state = TorrentItemState.Downloading;
                break;

            case TorrentState.Seeding:
                state = TorrentItemState.Seeding;
                break;

            case TorrentState.Stopping when _stopIntent == StopIntent.Pause:
                state = TorrentItemState.Pausing;
                break;

            case TorrentState.Stopping when _stopIntent == StopIntent.Queue:
                state = TorrentItemState.Queueing;
                break;

            case TorrentState.Stopping when _stopIntent == StopIntent.Finish:
                state = TorrentItemState.Finishing;
                break;

            case TorrentState.Stopped when _stopIntent == StopIntent.Pause:
                state = TorrentItemState.Paused;
                break;

            case TorrentState.Stopped when _stopIntent == StopIntent.Queue:
                state = TorrentItemState.Queued;
                break;

            case TorrentState.Stopped when _stopIntent == StopIntent.Finish:
                state = TorrentItemState.Finished;
                break;

            case TorrentState.FetchingHashes:
            case TorrentState.HashingPaused:
            case TorrentState.Hashing:
                state = TorrentItemState.Hashing;
                break;

            case TorrentState.Metadata:
                state = TorrentItemState.Metadata;
                break;

            case TorrentState.Error:
                state = TorrentItemState.Error;
                break;

            case TorrentState.Paused:
                break;
        }

        return state;
    }
}
