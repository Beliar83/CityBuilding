using System;
using System.Collections.Generic;
using System.Linq;
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
                int remainingCapacity = itemStorage.Items.Aggregate(itemStorage.Capacity,
                    (remaining, item) => remaining - item.Value.CurrentCount);
                foreach (KeyValuePair<string, StoredItemData> items in itemStorage.Items.OrderByDescending(item =>
                    item.Value.Priority))
                {
                    string item = items.Key;
                    StoredItemData itemData = items.Value;
                    if (itemData.RequestUntil.HasValue)
                    {
                        int missing = itemData.RequestUntil.Value - itemData.CurrentCount;
                        if (remainingCapacity > 0 && missing > 0)
                        {
                            int adjustedMissing = Math.Min(missing, remainingCapacity);
                            messenger.SendMessageToEntityManager(new CreateWalkerWithMessage(new ItemRequest
                            {
                                Item = item,
                                Amount = adjustedMissing
                            }));
                            remainingCapacity -= adjustedMissing;
                        }
                    }

                    if (!itemData.EmptyUntil.HasValue)
                    {
                        continue;
                    }

                    int excess = itemData.CurrentCount - itemData.EmptyUntil.Value;
                    if (excess > 0)
                    {
                        messenger.SendMessageToEntityManager(new CreateWalkerWithMessage(new StorageQuery
                        {
                            Item = item,
                            Amount = excess
                        }));
                    }
                }
            }
        }
    }
}
