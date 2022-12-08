using MonoTorrent;
using MonoTorrent.Client;
using System.ComponentModel;

namespace QueueTorrent
{
    public class TorrentFileItem : ITorrentFileItem
    {
        private readonly TorrentItem _parent;
        private readonly ITorrentManagerFile _torrentManagerFile;
        private readonly PropertyChangeAggregator _aggregator;

        private Priority _priority;
        private long _bytesDownloaded;
        private double _progress;

        public event PropertyChangedEventHandler? PropertyChanged;
        public long Key { get; private set; }
        public string Path { get; private set; } = string.Empty;
        public long Length { get; private set; }

        public long BytesDownloaded
        {
            get => _bytesDownloaded;
            set
            {
                _aggregator.Assign(ref _bytesDownloaded, value);
            }
        }

        public Priority Priority
        {
            get => _priority;
            set
            {
                if (value != _priority)
                {
                    _aggregator.Assign(ref _priority, value);
                    _parent.SetFilePriority(_torrentManagerFile, value);
                }
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                _aggregator.Assign(ref _progress, value);
            }
        }

        public TorrentFileItem(TorrentItem parent, ITorrentManagerFile fi)
        {
            _aggregator = new(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("*")));

            _parent = parent;
            _torrentManagerFile = fi;
            Key = fi.OffsetInTorrent;
            Path = fi.Path;
            Length = fi.Length;
            Poll();
        }

        internal void Poll()
        {
            var bdl = _torrentManagerFile.BytesDownloaded();

            using (_aggregator.DeferredPropertyChangedBlock())
            {
                Priority = _torrentManagerFile.Priority;
                BytesDownloaded = bdl;
                Progress = _torrentManagerFile.Length > 0 ? Math.Round((double)bdl / (double)_torrentManagerFile.Length, 3) : 1.0;
            }
        }
    }
}
