namespace QueueTorrent
{
    public class DisposeAction : IDisposable
    {
        private int _isDisposed;
        private readonly Action _disposeAction;

        public DisposeAction(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                _disposeAction();
            }
        }
    }
}
