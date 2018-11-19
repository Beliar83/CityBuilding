using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.EntityComponentSystem
{
    public abstract class EntityCollection : IEnumerable<Entity>
    {
        /// <inheritdoc />
        public abstract IEnumerator<Entity> GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Entity GetEntityById(Guid id)
        {
            return this.FirstOrDefault(e => e.Id == id);
        }
    }
}
