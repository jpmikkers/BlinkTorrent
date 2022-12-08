using MonoTorrent;
using MonoTorrent.Client;
using System.ComponentModel;

namespace QueueTorrent
{
    public partial class TorrentItem
    {
        private TorrentItem(TorrentService parent, TorrentManager manager, PersistedTorrentState? pts = null)
        {
            Key = new TorrentKey { Key = Interlocked.Increment(ref _keySource) };
            _aggregator = new(changedSet => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("*")));

            _parent = parent;
            _manager = manager;
            _infoHashes = manager.InfoHashes;

            if (pts != null)
            {
                _persistedTorrentState = pts;
                _totalBytesReceivedBase = pts.BytesDownloaded;
                _totalBytesSentBase = pts.BytesUploaded;
                _queuePosition = pts.QueuePosition;

                switch (pts.State)
                {
                    case TorrentItemState.Finishing:
                    case TorrentItemState.Finished:
                        _stopIntent = StopIntent.Finish;
                        break;

                    case TorrentItemState.Pausing:
                    case TorrentItemState.Paused:
                        _stopIntent = StopIntent.Pause;
                        break;

                    default:
                        _stopIntent = StopIntent.Queue;
                        break;
                }
            }

            ConstructPersistPaths();
            AssignFields();
            manager.TorrentStateChanged += Manager_TorrentStateChanged;
        }

        public static async Task<TorrentItem> CreateFromPersistedTorrentState(TorrentService parent, ClientEngine engine, PersistedTorrentStateExt ptsext)
        {
            TorrentItem ti = null!;
            var pts = ptsext.PersistedTorrentState;
            var dlLocation = parent.DownloadFolderFullPath;

            if (ptsext.Torrent != null)
            {
                ti = new(parent, await engine.AddAsync(ptsext.Torrent, dlLocation), pts)
                {
                    _magnetPersisted = true,
                    _torrentPersisted = true
                };
            }
            else if (ptsext.MagnetLink != null)
            {
                ti = new(parent, await engine.AddAsync(ptsext.MagnetLink, dlLocation), pts)
                {
                    _magnetPersisted = true,
                    _torrentPersisted = false
                };
            }
            else
            {
                //ti = new(parent, await engine.AddAsync(
                //    new MagnetLink(InfoHash.FromHex(ptsext.PersistedTorrentState.InfoHash)), dlLocation), pts)
                //{
                //    _magnetPersisted = false,
                //    _torrentPersisted = false
                //};
            }
            await ti.TryAssignPersistedFilePriorities();
            return ti;
        }

        public static async Task<TorrentItem> CreateFromMagnet(TorrentService parent, ClientEngine engine, MagnetLink magnetLink)
        {
            var ti=new TorrentItem(parent, await engine.AddAsync(magnetLink, parent.DownloadFolderFullPath));
            await ti.TryAssignPersistedFilePriorities();
            return ti;
        }

        public static async Task<TorrentItem> CreateFromTorrent(TorrentService parent, ClientEngine engine, Torrent torrent)
        {
            var ti=new TorrentItem(parent, await engine.AddAsync(torrent, parent.DownloadFolderFullPath));
            await ti.TryAssignPersistedFilePriorities();
            return ti;
        }
    }
}
