using System;
using EntityComponentSystem;

namespace GameEngine.EntityComponentSystem
{
    public interface Component
    {
        Guid Id { get; }
        Entity Entity { get; }
    }
}
