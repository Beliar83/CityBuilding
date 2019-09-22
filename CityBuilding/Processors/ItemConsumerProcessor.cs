using System.Linq;
using CityBuilding.Components;
using CityBuilding.Messages;
using JetBrains.Annotations;
using Xenko.Engine;
using Xenko.Games;

namespace CityBuilding.Processors
{
    public class ItemConsumerProcessor : EntityProcessor<ItemConsumer>
    {
        [NotNull]
        private readonly Messenger messenger;
        
        public ItemConsumerProcessor([NotNull] Messenger messenger)
        {
            this.messenger = messenger;
        }

        /// <inheritdoc />
        public override void Update(GameTime time)
        {
            foreach (CreateWalkerWithMessage message in ComponentDatas.Values.SelectMany(itemConsumer =>
                from item in itemConsumer.NeededItems.Keys
                let neededItemData = itemConsumer.NeededItems[item]
                let thresholdDifference = neededItemData.CurrentCount - neededItemData.OrderThreshold
                where thresholdDifference < 0
                select new CreateWalkerWithMessage(new ItemRequest {Item = item, 
                    Amount = neededItemData.MaxCount - neededItemData.CurrentCount,})))
            {
                messenger.SendMessageToEntityManager(message);
            }
        }
    }
}
