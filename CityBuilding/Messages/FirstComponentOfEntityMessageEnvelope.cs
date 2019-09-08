using System;
using JetBrains.Annotations;
using Xenko.Engine;

namespace CityBuilding.Messages
{
    public class FirstComponentOfEntityMessageEnvelope<T> : ComponentOfEntityMessageEnvelope where T : EntityComponent 
    {
        public FirstComponentOfEntityMessageEnvelope(Guid entityId, [NotNull] object message) :
            base(entityId, message)
        { }

        /// <inheritdoc />
        public override EntityComponent  GetComponent(Entity entity)
        {
            return entity.Components.Get<T>();
        }
    }
}
