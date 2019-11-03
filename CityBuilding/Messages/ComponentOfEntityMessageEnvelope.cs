using System;
using JetBrains.Annotations;
using Xenko.Engine;

namespace CityBuilding.Messages
{
    public abstract class ComponentOfEntityMessageEnvelope : EntityMessageEnvelope
    {
        [NotNull]
        public abstract EntityComponent GetComponent([NotNull] Entity entity);

        /// <inheritdoc />
        protected ComponentOfEntityMessageEnvelope(Guid entityId, [NotNull] object message) :
            base(entityId, message) { }
    }
}
