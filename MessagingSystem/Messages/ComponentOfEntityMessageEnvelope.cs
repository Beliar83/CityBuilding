using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;
using System;

namespace MessagingSystem.Messages
{
    public abstract class ComponentOfEntityMessageEnvelope : EntityMessageEnvelope
    {
        [NotNull]
        public abstract Component GetComponent([NotNull] Entity entity);

        /// <inheritdoc />
        protected ComponentOfEntityMessageEnvelope(Guid entityId, [NotNull] object message) :
            base(entityId, message)
        { }
    }
}
