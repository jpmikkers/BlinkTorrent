using MonoTorrent;
using MonoTorrent.Client;
using System.ComponentModel;

namespace QueueTorrent
{
    public partial class TorrentItem
    {
        private PersistedTorrentState _persistedTorrentState = new PersistedTorrentState();
        private string _statePersistPath = string.Empty;
        private string _torrentPersistPath = string.Empty;
        private string _magnetPersistPath = string.Empty;
        private bool _magnetPersisted = false;
        private bool _torrentPersisted = false;
        private bool _persistedFilePrioritiesWereAssigned = false;

        private void ConstructPersistPaths()
        {
            string pname = string.IsNullOrEmpty(V1InfoHash) ? V2InfoHash : V1InfoHash;
            _statePersistPath = Path.Combine(_parent.StateFolderFullPath, $"{pname}.state");
            _torrentPersistPath = Path.ChangeExtension(_statePersistPath, ".torrent");
            _magnetPersistPath = Path.ChangeExtension(_statePersistPath, ".magnet");
        }

        private void DeletePersistFiles()
        {
            if (File.Exists(_torrentPersistPath))
            {
                File.Delete(_torrentPersistPath);
            }

            if (File.Exists(_magnetPersistPath))
            {
                File.Delete(_magnetPersistPath);
            }

            if (File.Exists(_statePersistPath))
            {
                File.Delete(_statePersistPath);
            }
        }

        private void PersistMagnet()
        {
            if (!_magnetPersisted)
            {
                _magnetPersisted = true;
                File.WriteAllText(Path.ChangeExtension(_magnetPersistPath, ".magnet"), _manager.MagnetLink.ToV1String());
            }
        }

        private void PersistTorrent()
        {
            if (!_torrentPersisted
                && _manager.HasMetadata
                && !string.IsNullOrWhiteSpace(_manager.MetadataPath)
                && File.Exists(_manager.MetadataPath))
            {
                _torrentPersisted = true;
                File.Copy(_manager.MetadataPath, Path.ChangeExtension(_torrentPersistPath, ".torrent"));
            }
        }

        private void PersistState()
        {
            var tmp = new PersistedTorrentState()
            {
                State = State,
                V1InfoHash = V1InfoHash,
                V2InfoHash = V2InfoHash,
                QueuePosition = QueuePosition,
                BytesDownloaded = TotalBytesReceived,
                BytesUploaded = TotalBytesSent,
                FilePriorities = new ValueCollection<PersistedFilePriority>(
                    Files
                    .Where(x => x.Priority != Priority.Normal)
                    .OrderBy(x => x.Key)
                    .Select(x => new PersistedFilePriority
                    {
                        Key = x.Key,
                        Priority = x.Priority,
                    })
                    .ToList())
            };

            if (!tmp.Equals(_persistedTorrentState))
            {
                _persistedTorrentState = tmp;
                _persistedTorrentState.Serialize(_statePersistPath);
            }

            PersistMagnet();
            PersistTorrent();
        }

        private async Task TryAssignPersistedFilePriorities()
        {
            if (!_persistedFilePrioritiesWereAssigned && _manager.HasMetadata && _manager.Files.Count > 0)
            {
                _persistedFilePrioritiesWereAssigned = true;
                foreach (var prio in _persistedTorrentState.FilePriorities)
                {
                    var itf = _manager.Files.FirstOrDefault(x => x.OffsetInTorrent == prio.Key);
                    if (itf != null)
                    {
                        await _manager.SetFilePriorityAsync(itf, prio.Priority);
                    }
                }
            }
        }
    }
}
