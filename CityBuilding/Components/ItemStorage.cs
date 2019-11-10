using System.Collections.Generic;
using CityBuilding.Items;
using Xenko.Engine;

namespace CityBuilding.Components
{
    public class ItemStorage : EntityComponent
    {
        public ItemStorage(Dictionary<string, StoredItemData> storedItems = null)
        {
            Items = storedItems ?? new Dictionary<string, StoredItemData>();
        }

        public int? Capacity { get; set; }
        public bool AutomaticallyControlled { get; set; }
        public Dictionary<string, StoredItemData> Items { get; }
    }
}
