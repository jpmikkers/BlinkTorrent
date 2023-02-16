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
        private void InitHotfolder()
        {
            if (_settings.UseTorrentHotFolder)
            {
                _watcher = new FileSystemWatcher(_torrentHotFolderFullPath, "*.torrent");
                _watcher.Changed += FileSystemWatcher_Changed;
                _watcher.Created += FileSystemWatcher_Created;
                _watcher.EnableRaisingEvents = true;
            }
        }

        private async void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            await TryWatchedTorrentAdd(e.FullPath);
        }

        private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            await TryWatchedTorrentAdd(e.FullPath);
        }

        private async Task TryWatchedTorrentAdd(string torrentFullPath)
        {
            try
            {
                if (File.Exists(torrentFullPath))
                {
                    if (Torrent.TryLoad(torrentFullPath, out var torrent))
                    {
                        var newname = Path.ChangeExtension(torrentFullPath, ".torrent.added");
                        File.Copy(torrentFullPath, newname, true);
                        File.Delete(torrentFullPath);
                        await this.DownloadTorrent(torrent);
                        if (_settings.DeleteHotFolderTorrentAfterAdding)
                        {
                            File.Delete(newname);
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
