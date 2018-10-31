using System;
using System.Collections.Generic;

namespace GameEngine.EntityComponentSystem
{
    public interface Entity
    {
        Guid Id { get; }
        IEnumerable<Component> Components { get; }
    }
}
