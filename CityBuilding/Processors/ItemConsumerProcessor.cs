using System;
using System.Collections.Generic;
using CityBuilding.Components;
using CityBuilding.Items;
using CityBuilding.Messages;
using JetBrains.Annotations;
using Xenko.Engine;
using Xenko.Games;

namespace CityBuilding.Processors
{
    public class ItemConsumerProcessor : EntityProcessor<ItemConsumer, ItemConsumerAssociatedData>
    {
        [NotNull] private readonly Messenger messenger;

        private TimeSpan totalElapsed;

        public ItemConsumerProcessor([NotNull] Messenger messenger)
        {
            this.messenger = messenger;
        }

        /// <inheritdoc />
        public override void Update(GameTime time)
        {
            totalElapsed += time.Elapsed;
            while (totalElapsed.TotalSeconds >= 1)
            {
                foreach (KeyValuePair<ItemConsumer, ItemConsumerAssociatedData> keyValuePair in ComponentDatas)
                {
                    ItemConsumer itemConsumer = keyValuePair.Key;
                    ItemStorage itemStorage = keyValuePair.Value.ItemStorage;
                    foreach (KeyValuePair<string, NeededItemData> itemConsumerNeededItem in itemConsumer.NeededItems)
                    {
                        string key = itemConsumerNeededItem.Key;
                        NeededItemData neededItemData = itemConsumerNeededItem.Value;
                        if (itemStorage.Items.ContainsKey(key) &&
                            itemStorage.Items[key].CurrentCount >= neededItemData.OrderThreshold)
                        {
                            itemStorage.Items[key].CurrentCount -= neededItemData.ConsumptionPerSecond;
                        }
                    }
                }

                totalElapsed = totalElapsed.Subtract(new TimeSpan(0, 0, 1));
            }

            foreach (KeyValuePair<ItemConsumer, ItemConsumerAssociatedData> keyValuePair in ComponentDatas)
            foreach (string item in keyValuePair.Key.NeededItems.Keys)
            {
                ItemConsumer itemConsumer = keyValuePair.Key;
                ItemStorage itemStorage = keyValuePair.Value.ItemStorage;
                if (!itemStorage.Items.ContainsKey(item))
                {
                    itemStorage.Items[item] = new StoredItemData();
                }

                StoredItemData storedItem = itemStorage.Items[item];
                NeededItemData neededItemData = itemConsumer.NeededItems[item];

                int thresholdDifference = storedItem.CurrentCount - neededItemData.OrderThreshold;
                if (thresholdDifference >= 0)
                {
                    continue;
                }

                int capacity = Math.Min(itemStorage.Capacity ?? int.MaxValue, storedItem.MaxCount ?? int.MaxValue);
                var message = new CreateWalkerWithMessage(new ItemRequest
                    {Item = item, Amount = capacity - storedItem.CurrentCount});
                messenger.SendMessageToEntityManager(message);
            }
        }

        /// <inheritdoc />
        protected override ItemConsumerAssociatedData GenerateComponentData(Entity entity, ItemConsumer component)
        {
            return new ItemConsumerAssociatedData
            {
                ItemStorage = entity.Components.Get<ItemStorage>()
            };
        }
    }
}
