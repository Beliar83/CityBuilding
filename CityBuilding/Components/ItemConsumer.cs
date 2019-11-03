using System.Collections.Generic;
using CityBuilding.Items;
using Xenko.Engine;

namespace CityBuilding.Components
{
    public class ItemConsumer : EntityComponent
    {
        public ItemConsumer(Dictionary<string, NeededItemData> neededItems = null)
        {
            NeededItems = neededItems ?? new Dictionary<string, NeededItemData>();
        }

        public Dictionary<string, NeededItemData> NeededItems { get; }
    }
}
