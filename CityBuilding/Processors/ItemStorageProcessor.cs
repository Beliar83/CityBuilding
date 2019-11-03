using System.Collections.Generic;
using CityBuilding.Components;
using CityBuilding.Items;
using CityBuilding.Messages;
using JetBrains.Annotations;
using Xenko.Engine;
using Xenko.Games;

namespace CityBuilding.Processors
{
    public class ItemStorageProcessor : EntityProcessor<ItemStorage>
    {
        [NotNull] private readonly Messenger messenger;

        public ItemStorageProcessor([NotNull] Messenger messenger)
        {
            this.messenger = messenger;
        }

        /// <inheritdoc />
        public override void Update(GameTime time)
        {
            foreach (ItemStorage itemStorage in ComponentDatas.Values)
            {
                foreach (KeyValuePair<string, StoredItemData> items in itemStorage.Items)
                {
                    string item = items.Key;
                    StoredItemData itemData = items.Value;
                    if (!itemData.RequestUntil.HasValue)
                    {
                        continue;
                    }

                    int missing = itemData.RequestUntil.Value - itemData.CurrentCount;
                    if (missing > 0)
                    {
                        messenger.SendMessageToEntityManager(new CreateWalkerWithMessage(new ItemRequest
                        {
                            Item = item,
                            Amount = missing
                        }));
                    }
                }
            }
        }
    }
}
