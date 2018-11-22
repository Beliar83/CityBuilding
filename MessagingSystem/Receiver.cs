using JetBrains.Annotations;

namespace MessagingSystem
{
    public interface Receiver
    {
        void SetupReceive([NotNull] ReceiveDefinition receiveDefinition);
        void SetupReceive([NotNull] string state, [NotNull] ReceiveDefinition receiveDefinition);
    }
}
