using JetBrains.Annotations;
using System;

namespace MessagingSystem
{
    public interface Receiver
    {
        void SetupReceive<T>([NotNull] Action<T> receiveHandler, [CanBeNull] Predicate<T> receiveCondition = null);
    }
}
