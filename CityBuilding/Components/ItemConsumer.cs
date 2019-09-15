using System.Collections.Generic;
using CityBuilding.Items;
using Xenko.Engine;

namespace CityBuilding.Components
{
    public class ItemConsumer : EntityComponent
    {
        public Dictionary<string, NeededItemData> NeededItems { get; protected set; }
            = new Dictionary<string, NeededItemData>();
    }
}
