namespace QueueTorrent
{
    public interface ITorrentPeer
    {
        Uri Uri { get; }
        bool AmChoking { get; }
        bool AmInterested { get; }
        int AmRequestingPieceCount { get; }
        string Client { get; }
        TorrentPeer.Direction ConnectionDirection { get; }
        bool IsChoking { get; }
        bool IsConnected { get; }
        bool IsInterested { get; }
        int IsRequestingPieceCount { get; }
        bool IsSeeder { get; }
        int PiecesReceived { get; }
        int PiecesSent { get; }
    }
}
