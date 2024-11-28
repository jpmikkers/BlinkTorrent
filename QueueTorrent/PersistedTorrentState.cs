using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MonoTorrent;

namespace QueueTorrent;

public record PersistedTorrentStateExt
{
    public PersistedTorrentState PersistedTorrentState { get; set; } = default!;
    public MagnetLink? MagnetLink { get; set; }
    public Torrent? Torrent { get; set; }

    public static PersistedTorrentStateExt? Deserialize(string path)
    {
        var pts = PersistedTorrentState.Deserialize(path);

        if(pts==null) return null;

        var result = new PersistedTorrentStateExt();
        result.PersistedTorrentState = pts;

        string torrentPath = Path.ChangeExtension(path, ".torrent");
        if(File.Exists(torrentPath))
        {
            result.Torrent = Torrent.Load(torrentPath);
        }

        string magnetPath = Path.ChangeExtension(path, ".magnet");
        if (File.Exists(magnetPath))
        {
            result.MagnetLink = MagnetLink.Parse( File.ReadAllText(magnetPath) );   
        }

        return result;
    }
}

public record PersistedFilePriority
{
    public long Key { get; set; }
    public Priority Priority { get; set; }
}

public record PersistedTorrentState
{
    public TorrentItemState State { get; set; } = TorrentItemState.Queued;
    public string V1InfoHash { get; set; } = string.Empty;
    public string V2InfoHash { get; set; } = string.Empty;
    public int QueuePosition { get; set; }
    public long BytesDownloaded { get; set; }
    public long BytesUploaded { get; set; }
    public ValueCollection<PersistedFilePriority> FilePriorities { get; set; } = new();

    public static PersistedTorrentState? Deserialize(string path)
    {
        using var s = File.OpenRead(path);
        return JsonSerializer.Deserialize<PersistedTorrentState>(s,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            });
    }

    public void Serialize(string path)
    {
        using var s = File.Create(path);
        JsonSerializer.Serialize<PersistedTorrentState>(s, this, 
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                    {
                        new JsonStringEnumConverter()
                    }
            });
    }
}
