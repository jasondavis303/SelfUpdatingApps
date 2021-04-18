using System;

namespace SelfUpdatingApp
{
    class WaitProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;

        public WaitProgress(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            _handler = handler;
        }

        void IProgress<T>.Report(T value) => _handler.Invoke(value);
    }
}
