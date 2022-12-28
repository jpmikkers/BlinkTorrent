using Baksteen.Extensions.DeepCopy;
using MonoTorrent;
using MonoTorrent.Client;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Security.AccessControl;
using System.Text.Json;
using System.Timers;

namespace QueueTorrent
{
    public partial class TorrentService : ITorrentService
    {
        private const string BlinkTorrentStateFolder = "BTState";
        private const string MonoTorrentCacheFolder = "MTCache";

        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private TorrentSettings _settings = new TorrentSettings();
        private SerialQueue _serialQueue = new();
        private FileSystemWatcher _watcher = new();
        private ClientEngine _engine;
        internal List<TorrentItem> _torrents = new();

        private readonly string _baseFolderFullPath;
        private string _settingsFullPath = String.Empty;
        private string _stateFolderFullPath = String.Empty;
        private string _downloadFolderFullPath = String.Empty;
        private string _monoTorrentCacheDirectoryFullPath = String.Empty;
        private string _torrentHotFolderFullPath = String.Empty;
        internal string StateFolderFullPath => _stateFolderFullPath;
        internal string DownloadFolderFullPath => _downloadFolderFullPath;

        public event PropertyChangedEventHandler? PropertyChanged;

        public List<TorrentItem> Torrents
        {
            get
            {
                lock (_torrents)
                {
                    return _torrents.ToList();
                }
            }
        }

        public TorrentService(string basePath)
        {
            _baseFolderFullPath = Path.GetFullPath(basePath);
            _settingsFullPath = Path.Combine(_baseFolderFullPath, "queuetorrent.settings.json");

            if (File.Exists(_settingsFullPath))
            {
                using (var s = File.OpenRead(_settingsFullPath))
                {
                    _settings = JsonSerializer.Deserialize<TorrentSettings>(s) ?? new TorrentSettings();
                }
            }
            else
            {
                _settings = new TorrentSettings();
                SaveSettings();
            }

            _engine = new ClientEngine();
        }

        private void SaveSettings()
        {
            using(var s = File.Create(_settingsFullPath))
            {
                JsonSerializer.Serialize<TorrentSettings>(s, _settings, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });
            }
        }

        private string ConstructFullPath(string relativeOrAbsoluteFolder)
        {
            string result;
            if(Path.IsPathRooted(relativeOrAbsoluteFolder))
            {
                result = Path.GetFullPath(relativeOrAbsoluteFolder);
                if(!Directory.Exists(result))
                {
                    throw new ArgumentException($"Directory '{result}' does not exist.");
                }
            }
            else
            {
                result = Path.GetFullPath(Path.Combine(_baseFolderFullPath, relativeOrAbsoluteFolder));
                if(!Directory.Exists(result))
                {
                    Directory.CreateDirectory(result);
                }
            }
            return result;
        }

        private EngineSettings ConvertSettings(TorrentSettings settings)
        {
            var settingBuilder = new EngineSettingsBuilder
            {
                // Allow the engine to automatically forward ports using upnp/nat-pmp (if a compatible router is available)
                AllowPortForwarding = settings.AllowPortForwarding,

                // Automatically save a cache of the DHT table when all torrents are stopped.
                AutoSaveLoadDhtCache = true,

                // Automatically save 'FastResume' data when TorrentManager.StopAsync is invoked, automatically load it
                // before hash checking the torrent. Fast Resume data will be loaded as part of 'engine.AddAsync' if
                // torrent metadata is available. Otherwise, if a magnetlink is used to download a torrent, fast resume
                // data will be loaded after the metadata has been downloaded. 
                AutoSaveLoadFastResume = true,

                // If a MagnetLink is used to download a torrent, the engine will try to load a copy of the metadata
                // it's cache directory. Otherwise the metadata will be downloaded and stored in the cache directory
                // so it can be reloaded later.
                AutoSaveLoadMagnetLinkMetadata = true,

                DhtEndPoint = settings.DhtEndPoint,            // null => disabled Dht                
                ListenEndPoint = settings.ListenEndPoint,

                CacheDirectory = _monoTorrentCacheDirectoryFullPath,

                MaximumDownloadRate = settings.MaximumDownloadRate,
                MaximumUploadRate = settings.MaximumUploadRate,

                AllowLocalPeerDiscovery = settings.AllowLocalPeerDiscovery,
            };

            return settingBuilder.ToSettings();
        }

        private async Task StartInternal()
        {
            _stateFolderFullPath = ConstructFullPath(BlinkTorrentStateFolder);
            _monoTorrentCacheDirectoryFullPath = ConstructFullPath(MonoTorrentCacheFolder);
            _downloadFolderFullPath = ConstructFullPath(_settings.DownloadFolder);
            _torrentHotFolderFullPath = ConstructFullPath(_settings.TorrentHotFolder);

            _engine.Dispose();
            _engine = new ClientEngine(ConvertSettings(_settings));

            foreach ((var persistedTorrentStateExt, var idx) in Directory
                .EnumerateFiles(StateFolderFullPath, "*.state")
                .Select(x => PersistedTorrentStateExt.Deserialize(x)!)
                .Where(x => x != null)
                .OrderBy(x => x.PersistedTorrentState.QueuePosition)
                .Select((x, i) => (x, i)))
            {
                var ti = await TorrentItem.CreateFromPersistedTorrentState(this, _engine, persistedTorrentStateExt);
                ti.QueuePosition = idx;
                _torrents.Add(ti);
            }

            if (_torrents.Count > 0)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
            }
            InitHotfolder();

            _timer.Elapsed += OnTimer;
            _timer.Interval = 1000;
            _timer.Start();
        }


        private void AssertStarted()
        {
            if (!_timer.Enabled)
            {
                throw new InvalidOperationException("TorrentService not started yet");
            }
        }

        public async Task Start()
        {
            await _serialQueue.Enqueue(StartInternal);
        }

        public async Task DownloadMagnet(string uri)
        {
            await _serialQueue.Enqueue(async () =>
            {
                AssertStarted();
                if (MagnetLink.TryParse(uri, out MagnetLink? link))
                {
                    if (!_engine.Torrents.Any(x => x.InfoHashes == link.InfoHashes))
                    {
                        var ti = await TorrentItem.CreateFromMagnet(this, _engine, link);
                        ti.QueuePosition = _torrents.Count;
                        _torrents.Add(ti);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
                    }
                }
            });
        }

        public async Task DownloadTorrent(Stream torrentStream)
        {
            await _serialQueue.Enqueue(async () =>
            {
                AssertStarted();
                var torrent = await Torrent.LoadAsync(torrentStream);
                if (!_engine.Torrents.Any(x => x.InfoHashes == torrent.InfoHashes))
                {
                    var ti = await TorrentItem.CreateFromTorrent(this, _engine, torrent);
                    ti.QueuePosition = _torrents.Count;
                    _torrents.Add(ti);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
                }
            });
        }

        public async Task DownloadTorrent(Torrent torrent)
        {
            await _serialQueue.Enqueue(async () =>
            {
                AssertStarted();
                if (!_engine.Torrents.Any(x => x.InfoHashes == torrent.InfoHashes))
                {
                    var ti = await TorrentItem.CreateFromTorrent(this, _engine, torrent);
                    ti.QueuePosition = _torrents.Count;
                    _torrents.Add(ti);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
                }
            });
        }

        internal async Task RemoveKeep(TorrentItem torrent, bool keepFiles)
        {
            await _serialQueue.Enqueue(async () =>
            {
                await torrent.RemoveKeepInternal(keepFiles);
                // create a new list without the given torrents
                lock (_torrents)
                {
                    _torrents = _torrents.Where(x => x != torrent).Select(Renumber).ToList();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
            });
        }

        private async Task Poll()
        {
            //bool collectionModified = false;
            //var engineTorrents = _engine.Torrents.ToList();
            //var engineTorrentsInfoHashes = engineTorrents.Select(x => x.InfoHashes).ToHashSet();

            //// create a new list with the original order but without torrents that are no longer available
            //lock (_torrents)
            //{
            //    _torrents = _torrents.Where(x =>
            //    {
            //        if(engineTorrentsInfoHashes.Contains(x.InfoHashes))
            //        {
            //            return true;
            //        }
            //        else
            //        {
            //            collectionModified = true;
            //            return false;
            //        }
            //    }).Select(Renumber).ToList();
            //}

            //if (collectionModified)
            //{
            //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
            //}

            foreach (var ti in Torrents)
            {
                ti.Poll();
            }

            //foreach (var ti in Torrents)
            //{
            //    await ti.CheckForCompletionMove();
            //}

            await HandleQueueRules();
        }

        private async void OnTimer(object? sender, ElapsedEventArgs args)
        {
            try
            {
                await _serialQueue.Enqueue(Poll);
            }
            catch(Exception ex)
            {
                Trace.WriteLine($"BT Poll failed: {ex}");
            }
        }

        public void Dispose()
        {
            _watcher.Dispose();
            _timer.Stop();
            _timer.Elapsed -= OnTimer;
        }

        public async Task DownloadTorrentOrMagnet(Uri uri)
        {
            if(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                using(var client = new HttpClient())
                {
                    using(var m = new MemoryStream())
                    {
                        using(var s = await client.GetStreamAsync(uri))
                        {
                            await s.CopyToAsync(m);
                        }
                        m.Position = 0;
                        await this.DownloadTorrent(m);
                    }
                }
            }
            else if(uri.Scheme == "magnet")
            {
                await this.DownloadMagnet(uri.AbsoluteUri);
            }
        }

        public TorrentSettings GetSettings()
        {
            return _settings.DeepCopy()!;
        }

        public TorrentSettings GetDefaultSettings()
        {
            return new TorrentSettings();
        }

        public async Task SetSettings(TorrentSettings settings)
        {
            await _engine.UpdateSettingsAsync(ConvertSettings(settings));
            _settings = settings;
            SaveSettings();
        }
	}
}
