using MonoTorrent;
using MonoTorrent.Client;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Diagnostics;
using MonoTorrent.Trackers;

namespace QueueTorrent
{
    public partial class TorrentItem : ITorrentItem
    {
        public enum StopIntent
        {
            Pause,
            Queue,
            Finish
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly TorrentService _parent;
        private TorrentManager _manager;
        private InfoHashes _infoHashes;
        private string _name = string.Empty;
        private TorrentItemState _state;
        private TorrentState _rawState;
        private double _progress;
        private double _seedRatio;
        private long _uploadSpeed;
        private long _downloadSpeed;
        private bool _complete;
        private string _error = string.Empty;
        private int _numberOfSeeders;
        private int _numberOfLeechers;
        private int _queuePosition;

        private long _totalBytesReceivedBase;
        private long _totalBytesSentBase;

        private long _totalBytesDownloaded;
        private long _totalBytesUploaded;

        private StopIntent _stopIntent = StopIntent.Queue;

        internal SerialQueue ExecutionQueue { get; private set; } = new();

        private bool _processing = false;
        private int _busyNestCounter = 0;

        private readonly PropertyChangeAggregator _aggregator;

        private static long _keySource;

        public TorrentKey Key { get; private set; }
        public int QueuePosition { get => _queuePosition; set => _aggregator.Assign(ref _queuePosition, value); }
        public string Name { get => _name; private set => _aggregator.Assign(ref _name, value); }
        public TorrentItemState State { get => _state; private set => _aggregator.Assign(ref _state, value); }
        public TorrentState RawState { get => _rawState; private set => _aggregator.Assign(ref _rawState, value); }
        public double Progress { get => _progress; private set => _aggregator.Assign(ref _progress, value); }
        public long UploadRate { get => _uploadSpeed; private set => _aggregator.Assign(ref _uploadSpeed, value); }
        public long DownloadRate { get => _downloadSpeed; private set => _aggregator.Assign(ref _downloadSpeed, value); }
        public bool Complete { get => _complete; private set => _aggregator.Assign(ref _complete, value); }
        public string Error { get => _error; private set => _aggregator.Assign(ref _error, value); }
        public int NumberOfSeeders { get => _numberOfSeeders; private set => _aggregator.Assign(ref _numberOfSeeders, value); }
        public int NumberOfLeechers { get => _numberOfLeechers; private set => _aggregator.Assign(ref _numberOfLeechers, value); }
        public long TotalBytesReceived { get => _totalBytesDownloaded; private set => _aggregator.Assign(ref _totalBytesDownloaded, value); }
        public long TotalBytesSent { get => _totalBytesUploaded; private set => _aggregator.Assign(ref _totalBytesUploaded, value); }
        public double SeedRatio { get => _seedRatio; private set => _aggregator.Assign(ref _seedRatio, value); }
        public bool IsBusy { get => _processing; private set => _aggregator.Assign(ref _processing, value); }

        private List<TorrentFileItem> _files = new();
        public IEnumerable<ITorrentFileItem> Files { get => _files; }

        public string V1InfoHash {  get => _infoHashes.V1?.ToHex() ?? string.Empty; }

        public string V2InfoHash { get => _infoHashes.V2?.ToHex() ?? string.Empty; }

        private IDisposable BusyBlock()
        {
            _busyNestCounter++;
            IsBusy = true;
            return new DisposeAction(() =>
            {
                if (_busyNestCounter > 0)
                {
                    _busyNestCounter--;
                    if (_busyNestCounter == 0)
                    {
                        IsBusy = false;
                    }
                }
            });
        }

        internal void SetFilePriority(ITorrentManagerFile tfi, Priority prio)
        {
            Task.Run(async () => await _manager.SetFilePriorityAsync(tfi, prio)).Wait();
        }

        private void Manager_TorrentStateChanged(object? sender, TorrentStateChangedEventArgs e)
        {
            using (_aggregator.DeferredPropertyChangedBlock())
            {
                // Trace.WriteLine($"BT TorrentStateChanged: {Name} {e.OldState} {e.NewState}");
                Complete = _manager.Complete;
                RawState = _manager.State;      // todo: should I use e.NewState here?
                State = GetState();
            }
        }

        private void AssignFields()
        {
            Name = _manager.Torrent?.Name ?? _manager.InfoHashes.V1OrV2.ToHex();
            Complete = _manager.Complete;
            RawState = _manager.State;
            State = GetState();

            Progress = Math.Round(_manager.PartialProgress / 100.0, 4);
            UploadRate = _manager.Monitor.UploadRate;
            DownloadRate = _manager.Monitor.DownloadRate;
            NumberOfLeechers = _manager.Peers.Leechs;
            NumberOfSeeders = _manager.Peers.Seeds;

            var tbu = _totalBytesSentBase + _manager.Monitor.DataBytesSent;
            if (tbu < TotalBytesSent)
            {
                _totalBytesSentBase = TotalBytesSent;
            }
            else
            {
                TotalBytesSent = tbu;
            }

            var tbd = _totalBytesReceivedBase + _manager.Monitor.DataBytesReceived;
            if (tbd < TotalBytesReceived)
            {
                _totalBytesReceivedBase = TotalBytesReceived;
            }
            else
            {
                TotalBytesReceived = tbd;
            }

            if (_manager.HasMetadata)
            {
                if (_files.Count == 0)
                {
                    _files = _manager.Files.Select(x => new TorrentFileItem(this, x)).ToList();
                }
                else
                {
                    foreach (var file in _files)
                    {
                        file.Poll();
                    }
                }

                long totalBytes = _manager.Files
                    .Where(x => x.Priority != Priority.DoNotDownload)
                    .Select(x => x.Length)
                    .Sum();

                if (totalBytes > 0)
                {
                    SeedRatio = Math.Round((double)TotalBytesSent / (double)totalBytes, 4);
                }
                else
                {
                    SeedRatio = 0.0;
                }
            }
        }

        internal void Poll()
        {
            using (_aggregator.DeferredPropertyChangedBlock())
            {
                AssignFields();
            }
            PersistState();
        }

#if NEVER
        internal async Task CheckForCompletionMove()
        {
            if (this.Complete && !_downloadAndMoveCompleted)
            {
                _downloadAndMoveCompleted = true;

                await _manager.StopAsync(TimeSpan.FromSeconds(1.0));

                if(_manager.Files.Count > 1)
                {
                    var targetFolder = Path.Combine(_parent.CompleteFolderFullPath, Name);
                    if(!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }
                    await _manager.MoveFilesAsync(targetFolder, true);
                }
                else
                {
                    await _manager.MoveFilesAsync(_parent.CompleteFolderFullPath, true);
                }

                await _manager.StartAsync();
            }
        }
#endif

        public async Task Pause()
        {
            await ExecutionQueue.Enqueue(async () =>
            {
                if (State != TorrentItemState.Paused && State != TorrentItemState.Pausing)
                {
                    using (BusyBlock())
                    {
                        _stopIntent = StopIntent.Pause;
                        await _manager.StopAsync(TimeSpan.FromSeconds(1.0));
                    }
                }
            });
        }

        public async Task Resume()
        {
            await ExecutionQueue.Enqueue(async () =>
            {
                if (State == TorrentItemState.Paused)
                {
                    using (BusyBlock())
                    {
                        _stopIntent = StopIntent.Queue;
                        await Task.Delay(1);
                    }
                }
            });
        }

        public async Task Remove(bool keepFiles)
        {
            using (BusyBlock())
            {
                await _parent.RemoveKeep(this, keepFiles);
            }
        }

        public async Task Verify()
        {
            await ExecutionQueue.Enqueue(async () =>
            {
                using (BusyBlock())
                {
                    try
                    {
                        if (_manager.State == TorrentState.Hashing)
                        {
                        }
                        else if (_manager.State != TorrentState.Stopped)
                        {
                            await _manager.StopAsync(TimeSpan.FromSeconds(1.0));
                            await _manager.HashCheckAsync(true);
                        }
                        else
                        {
                            await _manager.HashCheckAsync(false);
                        }
                    }
                    catch
                    {
                    }
                }
            });
        }

        internal async Task RemoveKeepInternal(bool keepFiles)
        {
            try
            {
                _stopIntent = StopIntent.Pause;
                await _manager.StopAsync(TimeSpan.FromSeconds(1.0));
                if (!keepFiles)
                {
                    foreach (var file in _manager.Files)
                    {
                        try
                        {
                            if (File.Exists(file.FullPath))
                            {
                                File.Delete(file.FullPath);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    if(_manager.Files.Count>1 && Directory.Exists(_manager.ContainingDirectory))
                    {
                        Directory.Delete(_manager.ContainingDirectory, true);
                    }
                }
                await _manager.Engine.RemoveAsync(_manager);
            }
            catch
            {
            }

            DeletePersistFiles();
        }

        internal async Task QueueInternal()
        {
            await ExecutionQueue.Enqueue(async () =>
            {
                using (BusyBlock())
                {
                    _stopIntent = StopIntent.Queue;
                    try
                    {
                        await _manager.StopAsync(TimeSpan.FromSeconds(1.0));
                    }
                    catch
                    {
                    }
                }
            });
        }

        internal async Task StartInternal()
        {
            await ExecutionQueue.Enqueue(async () =>
            {
                using (BusyBlock())
                {
                    await StartAsyncWaitForStarted();
                }
            });
        }

        internal async Task FinishInternal()
        {
            await ExecutionQueue.Enqueue(async () =>
            {
                using (BusyBlock())
                {
                    _stopIntent = StopIntent.Finish;
                    await _manager.StopAsync();
                }
            });
        }

        private async Task StartAsyncWaitForStarted()
        {
            int t = 0;
            try
            {
                await _manager.StartAsync();
                while (t < 100 && _manager.State == TorrentState.Starting)
                {
                    t++;
                    await Task.Delay(100);
                }
            }
            catch
            {
            }
        }

		public async Task QueueDown()
		{
            await _parent.QueueDown(new[] { this.Key });
		}

		public async Task QueueToBottom()
		{
			await _parent.QueueToBottom(new[] { this.Key });
		}

		public async Task QueueToTop()
		{
			await _parent.QueueToTop(new[] { this.Key });
		}

		public async Task QueueUp()
		{
			await _parent.QueueUp(new[] { this.Key });
		}

        public async Task<IEnumerable<ITorrentPeer>> GetPeers()
        {
            var peers = await _manager.GetPeersAsync();
            return peers.Select(p => new TorrentPeer
            {
                Uri = p.Uri,
                AmChoking= p.AmChoking,
                AmInterested= p.AmInterested,
                IsInterested= p.IsInterested,
                IsChoking= p.IsChoking,
                IsConnected= p.IsConnected,
                AmRequestingPieceCount= p.AmRequestingPiecesCount,
                IsRequestingPieceCount = p.IsRequestingPiecesCount,
                IsSeeder= p.IsSeeder,
                ConnectionDirection = (TorrentPeer.Direction)p.ConnectionDirection,
                PiecesReceived= p.PiecesReceived,
                PiecesSent= p.PiecesSent,   
                Client = p.ClientApp.Client.ToString()
            });
        }

        public async Task<IEnumerable<ITorrentTracker>> GetTrackers()
        {
            var trackersTmp = new List<TorrentTracker>();
            foreach(var (tier, level) in _manager.TrackerManager.Tiers.Select((t, i) => (t, i)))
            {
                foreach(var tracker in tier.Trackers)
                {
                    trackersTmp.Add(new()
                    {
                        Tier = level,
                        Uri = tracker.Uri,
                        Active = tier.ActiveTracker == tracker,
                        CanScrape = tracker.CanScrape,
                        MinUpdateInterval = tracker.MinUpdateInterval,
                        UpdateInterval = tracker.UpdateInterval,
                        TimeSinceLastAnnounce = tracker.TimeSinceLastAnnounce,
                        Status = tracker.Status.ToString(),
                        FailureMessage = tracker.FailureMessage,
                        WarningMessage = tracker.WarningMessage,
                    });
                }
            }
            await Task.CompletedTask;
            return trackersTmp;
        }
    }
}
