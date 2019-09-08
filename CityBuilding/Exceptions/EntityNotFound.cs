using System;
using JetBrains.Annotations;

namespace CityBuilding.Exceptions
{
    public class EntityNotFound : Exception
    {
        private readonly Guid entityId;

        internal EntityNotFound(Guid entityId)
        {
            this.entityId = entityId;
        }

        /// <inheritdoc />
        [NotNull]
        public override string Message => $"The entity with ID {entityId} was not with.";
    }
}
