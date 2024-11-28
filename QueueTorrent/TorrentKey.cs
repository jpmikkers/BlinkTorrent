namespace QueueTorrent;

	public readonly struct TorrentKey
{
    public /* required */ long Key { get; init; }

    public override string ToString()
    {
        return Key.ToString();
    }

    public static bool TryParse(string? s, out TorrentKey value)
    {
        if(int.TryParse(s,out int result))
        {
            value = new TorrentKey() { Key = result };
            return true;
        }
        else
        {
            value = default;
        }
        return false;
    }
}