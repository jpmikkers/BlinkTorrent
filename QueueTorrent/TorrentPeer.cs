namespace QueueTorrent
{
    public class TorrentPeer : ITorrentPeer
    {
        public enum Direction
        {
            None,
            Incoming,
            Outgoing
        }

        public Uri Uri { get; init; } = default!;

        public bool AmInterested { get; init; } = false;
        public bool AmChoking { get; init; } = false;
        public int AmRequestingPieceCount { get; init; }

        public bool IsConnected { get; init; } = false;
        public bool IsInterested { get; init; } = false;
        public bool IsChoking { get; init; } = false;
        public int IsRequestingPieceCount { get; init; }
        public bool IsSeeder { get; init; } = false;

        public Direction ConnectionDirection { get; init; } = Direction.None;

        public int PiecesReceived { get; init; } = 0;
        public int PiecesSent { get; init; } = 0;
        public string Client { get; init; } = string.Empty;
    }
}
