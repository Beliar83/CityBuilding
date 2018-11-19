using JetBrains.Annotations;
using System;

namespace MessagingSystem.Akka.Exceptions
{
    internal class EntityNotFound : Exception
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
