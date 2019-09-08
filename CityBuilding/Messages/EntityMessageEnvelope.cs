using System;
using JetBrains.Annotations;

namespace CityBuilding.Messages
{
    public class EntityMessageEnvelope
    {
        public EntityMessageEnvelope(Guid entityId, [NotNull] object message)
        {
            EntityId = entityId;
            Message = message;
        }

        public Guid EntityId { get; }

        [NotNull] public object Message { get; }
    }
}
