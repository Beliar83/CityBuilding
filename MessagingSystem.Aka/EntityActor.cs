using System.Collections.Generic;
using Akka.Actor;
using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;
using MessagingSystem.Messages;

namespace MessagingSystem.Akka
{
    public class EntityActor : ReceiveActor
    {
        [NotNull] private readonly Entity entity;
        [NotNull] private readonly Dictionary<string, List<ReceiveDefinition>> receiveDefinitions;

        public EntityActor([NotNull] Entity entity,
            [NotNull] Dictionary<string, List<ReceiveDefinition>> receiveDefinitions)
        {
            this.entity = entity;
            this.receiveDefinitions = receiveDefinitions;
            Receive<ChangeState>(HandleChangeState);
        }

        private bool HandleChangeState([NotNull] ChangeState message)
        {
            if (!receiveDefinitions.ContainsKey(message.NewState))
            {
                Sender.Tell(new StateNotConfigured(message.NewState));
                return false;
            }

            List<ReceiveDefinition> stateReceives = receiveDefinitions[message.NewState];
            Become(() =>
            {
                Receive<ChangeState>(HandleChangeState);
                foreach (ReceiveDefinition receiveDefinition in stateReceives)
                {
                    Receive(receiveDefinition.MessageType, receiveDefinition.ReceiveHandler,
                        receiveDefinition.ShouldHandle);
                }
            });
            return true;
        }

        [NotNull]
        public static Props GetProps([NotNull] Entity entity,
            [NotNull] Dictionary<string, List<ReceiveDefinition>> receiveDefinitions)
        {
            Props props = Props.Create(() => new EntityActor(entity, receiveDefinitions));
            return props;
        }
    }
}
