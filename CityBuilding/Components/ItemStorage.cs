using System.Collections.Generic;
using CityBuilding.Items;
using Xenko.Engine;

namespace CityBuilding.Components
{
    public class ItemStorage : EntityComponent
    {
        public int Capacity { get; set; }
        public Dictionary<string, StoredItemData> Items { get; protected set; } 
            = new Dictionary<string, StoredItemData>();
    }
}
