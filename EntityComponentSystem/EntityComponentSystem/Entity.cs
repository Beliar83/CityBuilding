using System;

namespace GameEngine.EntityComponentSystem
{
    public abstract class Entity : IEquatable<Entity>
    {
        public abstract Guid Id { get; }
        public abstract ComponentCollection Components { get; }

        /// <inheritdoc />
        public bool Equals(Entity other)
        {
            if (other is null)
            {
                return false;
            }

            return ReferenceEquals(this, other) || Id.Equals(other.Id);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((Entity) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
