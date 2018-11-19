using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;
using System;

namespace MessagingSystem.Messages
{
    public class FirstComponentOfEntityMessageEnvelope<T> : ComponentOfEntityMessageEnvelope where T : Component
    {
        public FirstComponentOfEntityMessageEnvelope(Guid entityId, [NotNull] object message) :
            base(entityId, message)
        { }

        /// <inheritdoc />
        public override Component GetComponent(Entity entity)
        {
            return entity.Components.GetFirstComponentOfType<T>();
        }
    }
}
