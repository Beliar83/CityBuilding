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
            foreach (KeyValuePair<ItemConsumer, ItemConsumerAssociatedData> keyValuePair in ComponentDatas)
            {
                ItemConsumer itemConsumer = keyValuePair.Key;
                ItemStorage itemStorage = keyValuePair.Value.ItemStorage;

                itemStorage.AutomaticallyControlled = true;
                foreach (string item in itemConsumer.NeededItems.Keys.Where(
                    item => !itemStorage.Items.ContainsKey(item)))
                {
                    itemStorage.Items[item] = new StoredItemData();
                }
            }

            totalElapsed += time.Elapsed;
            while (totalElapsed.TotalSeconds >= 1)
            {
                foreach (KeyValuePair<ItemConsumer, ItemConsumerAssociatedData> keyValuePair in ComponentDatas)
                {
                    ItemConsumer itemConsumer = keyValuePair.Key;
                    ItemStorage itemStorage = keyValuePair.Value.ItemStorage;
                    Activatable activatable = keyValuePair.Value.Activatable;
                    foreach (KeyValuePair<string, NeededItemData> itemConsumerNeededItem in itemConsumer.NeededItems)
                    {
                        string key = itemConsumerNeededItem.Key;
                        NeededItemData neededItemData = itemConsumerNeededItem.Value;
                        if (itemStorage.Items.ContainsKey(key) &&
                            itemStorage.Items[key].CurrentCount >= neededItemData.ConsumptionPerSecond)
                        {
                            itemStorage.Items[key].CurrentCount -= neededItemData.ConsumptionPerSecond;
                            activatable.Active = true;
                        }
                        else
                        {
                            activatable.Active = false;
                        }
                    }
                }

                totalElapsed = totalElapsed.Subtract(new TimeSpan(0, 0, 1));
            }

            foreach (CreateWalkerWithMessage message in ComponentDatas.SelectMany(keyValuePair =>
                from item in keyValuePair.Key.NeededItems.Keys
                let itemConsumer = keyValuePair.Key
                let itemStorage = keyValuePair.Value.ItemStorage
                let storedItem = itemStorage.Items[item]
                let neededItemData = itemConsumer.NeededItems[item]
                let thresholdDifference = storedItem.CurrentCount - neededItemData.OrderThreshold
                where thresholdDifference < 0
                let capacity = Math.Min(itemStorage.Capacity ?? int.MaxValue, storedItem.MaxCount ?? int.MaxValue)
                select new CreateWalkerWithMessage(new ItemRequest
                    {Item = item, Amount = capacity - storedItem.CurrentCount})))
            {
                messenger.SendMessageToEntityManager(message);
            }
        }

        /// <inheritdoc />
        protected override ItemConsumerAssociatedData GenerateComponentData(Entity entity, ItemConsumer component)
        {
            var itemStorage = entity.Components.Get<ItemStorage>();
            if (itemStorage is null)
            {
                itemStorage = new ItemStorage();
                entity.Components.Add(itemStorage);
            }

            var activatable = entity.Components.Get<Activatable>();
            if (!(activatable is null))
            {
                return new ItemConsumerAssociatedData
                {
                    ItemStorage = itemStorage,
                    Activatable = activatable
                };
            }

            activatable = new Activatable();
            entity.Components.Add(activatable);

            return new ItemConsumerAssociatedData
            {
                ItemStorage = itemStorage,
                Activatable = activatable
            };
        }
    }
}
