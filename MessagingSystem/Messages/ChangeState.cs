using JetBrains.Annotations;

namespace MessagingSystem.Messages
{
    public class ChangeState
    {
        [NotNull] public string NewState { get; }

        public ChangeState([NotNull] string newState)
        {
            NewState = newState;
        }
    }
}
