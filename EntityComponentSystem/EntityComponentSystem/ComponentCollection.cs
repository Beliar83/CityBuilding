using System.Collections;
using System.Collections.Generic;

namespace GameEngine.EntityComponentSystem
{
    public abstract class ComponentCollection : IEnumerable<Component>
    {
        /// <inheritdoc />
        public abstract IEnumerator<Component> GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract T GetFirstComponentOfType<T>() where T : Component;
        public abstract T GetComponentOfTypeAtIndex<T>(int index) where T : Component;
    }
}
