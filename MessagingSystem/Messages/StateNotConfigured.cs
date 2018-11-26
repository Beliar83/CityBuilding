using JetBrains.Annotations;

namespace MessagingSystem.Messages
{
    public class StateNotConfigured
    {
        [NotNull] public string State { get; }

        public StateNotConfigured([NotNull] string state)
        {
            State = state;
        }
    }
}
