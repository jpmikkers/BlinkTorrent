using MonoTorrent;
using MonoTorrent.Client;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Timers;

namespace QueueTorrent
{
    public partial class TorrentService
    {
        private async Task HandleQueueRules()
        {
            bool IsQueued(TorrentItem item) => 
                item.State == TorrentItemState.Queued;

            bool IsSeeding(TorrentItem item) => 
                item.State == TorrentItemState.Seeding
                || item.State == TorrentItemState.Starting;

            bool IsDownloading(TorrentItem item) =>
                item.State == TorrentItemState.Downloading
                || item.State == TorrentItemState.Starting;

            var tl = Torrents;
            int seedingCount = 0;
            int downloadCount = 0;
            var moveToWaitingSet = new HashSet<TorrentItem>();
            var moveToStartSet = new HashSet<TorrentItem>();
            var moveToFinishedSet = new HashSet<TorrentItem>();

            foreach (var t in tl)
            {
                if (t.Complete)
                {
                    #region seeding rules
                    if (seedingCount < _settings.MaximumActiveUploads)
                    {
                        if (IsQueued(t))
                        {
                            if (t.SeedRatio >= _settings.SeedLimit)
                            {
                                moveToFinishedSet.Add(t);
                            }
                            else
                            {
                                moveToStartSet.Add(t);
                                seedingCount++;
                            }
                        }
                        else if (IsSeeding(t))
                        {
                            if (t.SeedRatio >= _settings.SeedLimit)
                            {
                                moveToFinishedSet.Add(t);
                            }
                            else
                            {
                                seedingCount++;
                            }
                        }
                    }
                    else
                    {
                        if (IsSeeding(t))
                        {
                            if (t.SeedRatio >= _settings.SeedLimit)
                            {
                                moveToFinishedSet.Add(t);
                            }
                            else
                            {
                                moveToWaitingSet.Add(t);
                            }
                        }
                    }
                    #endregion seeding rules
                }
                else
                {
                    #region download rules
                    if (downloadCount < _settings.MaximumActiveDownloads)
                    {
                        if (IsQueued(t))
                        {
                            moveToStartSet.Add(t);
                            downloadCount++;
                        }
                        else if (IsDownloading(t))
                        {
                            downloadCount++;
                        }
                    }
                    else
                    {
                        if (IsDownloading(t))
                        {
                            moveToWaitingSet.Add(t);
                        }
                    }
                    #endregion download rules
                }
            }

            foreach(var toFinish in moveToFinishedSet)
            {
                try
                {
                    await toFinish.FinishInternal();
                }
                catch
                {
                }
            }

            foreach(var toFinish in moveToWaitingSet)
            {
                try
                {
                    await toFinish.QueueInternal();
                }
                catch
                {
                }
            }

            foreach(var toFinish in moveToStartSet)
            {
                try
                {
                    await toFinish.StartInternal();
                }
                catch
                {
                }
            }
        }
    }
}
