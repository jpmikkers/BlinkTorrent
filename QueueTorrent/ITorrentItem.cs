using MonoTorrent;
using System.ComponentModel;
using static QueueTorrent.TorrentItem;

namespace QueueTorrent
{
    public enum TorrentItemState
    {
        Starting,           // MT Starting

        Downloading,        // MT Downloading
        Seeding,            // MT Seeding

        Pausing,            // MT Stopping && intent = pause
        Queueing,           // MT Stopping && intent = queue
        Finishing,          // MT Stopping && intent = finish

        Paused,             // paused torrents will not be considered in queueing
        Queued,             // queued torrents are automatically started and stopped according to queueing limits and priorities
        Finished,           // torrent downloaded fully and seed ratio reached

        Hashing,
        Metadata,
        Error,
    }

    public struct TorrentKey
    {
        public /* required */ long Key { get; init; }

        public override string ToString()
        {
            return Key.ToString();
        }

        public static bool TryParse(string? s, out TorrentKey value)
        {
            if(int.TryParse(s,out int result))
            {
                value = new TorrentKey() { Key = result };
                return true;
            }
            else
            {
                value = default;
            }
            return false;
        }
    }

    public interface ITorrentItem : INotifyPropertyChanged
    {
        TorrentKey Key { get; }

        string V1InfoHash { get; }
        string V2InfoHash { get; }

        //InfoHashes InfoHashes { get; }
        string Name { get; }

        TorrentItemState State { get; }
        string Error { get; }
        bool IsBusy { get; }
        bool Complete { get; }

        int QueuePosition { get; set; }

        double Progress { get; }
        double SeedRatio { get; }

        int NumberOfLeechers { get; }
        int NumberOfSeeders { get; }

        long DownloadRate { get; }
        long UploadRate { get; }

        long TotalBytesReceived { get; }
        long TotalBytesSent { get; }

		bool CanMoveUp { get; }
		bool CanMoveDown { get; }

		IEnumerable<ITorrentFileItem> Files { get; }

        Task Pause();
        Task Resume();
        Task Remove(bool keepFiles);
        Task Verify();

		Task QueueDown();
		Task QueueToBottom();
		Task QueueToTop();
		Task QueueUp();

        Task<IEnumerable<ITorrentPeer>> GetPeers();
        Task<IEnumerable<ITorrentTracker>> GetTrackers();
    }
}