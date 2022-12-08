using MonoTorrent;
using System.ComponentModel;

namespace QueueTorrent
{
    public interface ITorrentFileItem : INotifyPropertyChanged
    {
        long BytesDownloaded { get; set; }
        long Key { get; }
        long Length { get; }
        string Path { get; }
        Priority Priority { get; set; }
        double Progress { get; set; }
    }
}