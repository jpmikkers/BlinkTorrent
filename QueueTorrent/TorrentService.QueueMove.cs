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
        private TorrentItem Renumber(TorrentItem item, int i)
        {
            item.QueuePosition = i;
            return item;
        }

        private List<TorrentItem> ListOfKeysToListOfTorrentItems(IEnumerable<TorrentKey> keys)
        {
            return keys.Select(x => _torrents.FirstOrDefault(y => y.Key.Equals(x))).NotNull().ToList();
        }

        public async Task QueueToTop(IEnumerable<TorrentKey> keys)
        {
            await _serialQueue.Enqueue(() =>
            {
                AssertStarted();
                var items = ListOfKeysToListOfTorrentItems(keys);
                _torrents = items
                    .OrderBy(x => x.QueuePosition)
                    .Concat(_torrents.Except(items))
                    .Select(Renumber)
                    .ToList();
            });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
        }

        public async Task QueueToBottom(IEnumerable<TorrentKey> keys)
        {
            await _serialQueue.Enqueue(() =>
            {
                AssertStarted();
                var items = ListOfKeysToListOfTorrentItems(keys);
                _torrents = _torrents
                    .Except(items)
                    .Concat(items.OrderBy(x => x.QueuePosition))
                    .Select(Renumber)
                    .ToList();
            });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
        }

        public async Task QueueUp(IEnumerable<TorrentKey> keys)
        {
            await _serialQueue.Enqueue(() =>
            {
                AssertStarted();
                var items = ListOfKeysToListOfTorrentItems(keys);
                var pivot = items.Select(x => x.QueuePosition).Min() - 1;
                var head = _torrents.Except(items).Where(x => x.QueuePosition < pivot).ToList();
                var tail = _torrents.Except(items).Where(x => x.QueuePosition >= pivot).ToList();
                _torrents = head
                    .Concat(items.OrderBy(x => x.QueuePosition))
                    .Concat(tail)
                    .Select(Renumber)
                    .ToList();
            });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
        }

        public async Task QueueDown(IEnumerable<TorrentKey> keys)
        {
            await _serialQueue.Enqueue(() =>
            {
                AssertStarted();
                var items = ListOfKeysToListOfTorrentItems(keys);
                var pivot = items.Select(x => x.QueuePosition).Max() + 1;
                var head = _torrents.Except(items).Where(x => x.QueuePosition <= pivot).ToList();
                var tail = _torrents.Except(items).Where(x => x.QueuePosition > pivot).ToList();
                _torrents = head
                    .Concat(items.OrderBy(x => x.QueuePosition))
                    .Concat(tail)
                    .Select(Renumber)
                    .ToList();
            });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Torrents)));
        }
    }
}
