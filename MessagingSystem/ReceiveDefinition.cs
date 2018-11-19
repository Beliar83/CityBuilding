using System;
using JetBrains.Annotations;

namespace MessagingSystem
{
    public class ReceiveDefinition
    {
        [NotNull] public Type MessageType { get; }

        [NotNull] public Action<object> ReceiveHandler { get; }

        [NotNull] public Predicate<object> ShouldHandle { get; }

        private ReceiveDefinition([NotNull] Type messageType, [NotNull] Action<object> receiveHandler,
            [NotNull] Predicate<object> shouldHandle)
        {
            MessageType = messageType;
            ReceiveHandler = receiveHandler;
            ShouldHandle = shouldHandle;
        }
    }
}
