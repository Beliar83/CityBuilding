using System;
using Akka.Actor;
using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;
using MessagingSystem.Akka.Exceptions;

namespace MessagingSystem.Akka
{
    internal class EntityActor : ReceiveActor
    {
        [NotNull] private readonly Entity entity;

        private EntityActor([NotNull] Entity entity)
        {
            // ReSharper disable once TooManyChainedReferences
            Guid entityId = Guid.Parse(Self.Path.Name);
            this.entity = entity;
            if (entity is null)
            {
                throw new EntityNotFound(entityId);
            }
            
        }

        [Hyperion.Internal.NotNull]
        public static Props GetProps([NotNull] Entity entity)
        {
            return Props.Create(() => new EntityActor(entity));
        }
    }
}
