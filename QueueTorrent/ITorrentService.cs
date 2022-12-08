using MonoTorrent;
using System.ComponentModel;

namespace QueueTorrent
{
    public interface ITorrentService: IDisposable, INotifyPropertyChanged
    {
        TorrentSettings Settings { get; }
        List<TorrentItem> Torrents { get; }
        Task DownloadTorrentOrMagnet(Uri uri);
        Task DownloadMagnet(string uri);
        Task DownloadTorrent(Stream torrentStream);
        Task DownloadTorrent(Torrent torrent);
        Task QueueDown(IEnumerable<TorrentKey> items);
        Task QueueToBottom(IEnumerable<TorrentKey> items);
        Task QueueToTop(IEnumerable<TorrentKey> items);
        Task QueueUp(IEnumerable<TorrentKey> items);
        Task Start();
    }
}