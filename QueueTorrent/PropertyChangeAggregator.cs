using System.Runtime.CompilerServices;
using System.Linq;

namespace QueueTorrent
{
    public class PropertyChangeAggregator
    {
        private int _deferPropertyChangedNestCounter = 0;
        private HashSet<string> _modifiedProperties = new();
        private readonly Action<HashSet<string>> _onModifiedAction;

        public PropertyChangeAggregator(Action<HashSet<string>> onModified)
        {
            _onModifiedAction = onModified;
        }

        public IDisposable DeferredPropertyChangedBlock()
        {
            Interlocked.Increment(ref _deferPropertyChangedNestCounter);
            return new DisposeAction(() =>
            {
                if (Interlocked.Decrement(ref _deferPropertyChangedNestCounter) == 0)
                {
                    FireWhenModified();
                }
            });
        }

        public void Assign<T>(ref T dest, T value, [CallerMemberName] String propertyName = "")
        {
            if (dest != null && !dest.Equals(value))
            {
                dest = value;

                lock (_modifiedProperties)
                {
                    _modifiedProperties.Add(propertyName);
                }

                FireWhenModified();
            }
        }

        private void FireWhenModified()
        {
            if (Interlocked.CompareExchange(ref _deferPropertyChangedNestCounter, 0, 0) == 0)
            {
                var tmp = new HashSet<string>();
                lock (_modifiedProperties)
                {
                    tmp = _modifiedProperties.ToHashSet();
                    _modifiedProperties.Clear();
                }

                if (tmp.Count > 0)
                {
                    _onModifiedAction(tmp);
                }
            }
        }
    }
}
