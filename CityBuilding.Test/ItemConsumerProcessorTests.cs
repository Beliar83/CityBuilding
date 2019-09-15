using Akka.Actor;
using CityBuilding.Components;
using CityBuilding.Items;
using CityBuilding.Messages;
using CityBuilding.Processors;
using Xenko.Games;
using Xunit;
using Moq.Protected;
using Shouldly;

namespace CityBuilding.Test
{
    public class ItemConsumerProcessorTests : TestWithMessenger
    {

        [Theory]
        [InlineData("Test", 1, 10, 5, 4)]
        public void ItemConsumerAsksForWalkerWithItemRequestWhenBelowThreshold(string item, int currentCount, int maxCount, int orderThreshold, int expectedAmount)
        {
            GivenSceneExists();
            GivenMessengerExists();
            MessengerMock.Protected()
                .Setup<IActorRef>("CreateEntityManager")
                .Returns(() => TestActor);
            var itemConsumerProcessor = new ItemConsumerProcessor(Messenger);
            Game.SceneSystem.SceneInstance.Processors.Add(itemConsumerProcessor);
            GivenTestEntityExists();
            GivenTestEntityIsInList();
            var itemConsumer = new ItemConsumer
            {
                NeededItems =
                {
                    [item] = new NeededItemData
                        {CurrentCount = currentCount, MaxCount = maxCount, OrderThreshold = orderThreshold,}
                }
            };
            TestEntity.Components.Add(itemConsumer);
            Game.SceneSystem.Update(new GameTime());
            ExpectMsg<CreateWalkerWithMessage>(message => IsWalkerWithItemRequest(message, item, expectedAmount));
        }

        private static bool IsWalkerWithItemRequest(CreateWalkerWithMessage message, string expectedItem, int expectedAmount)
        {
            var request = message.Message.ShouldBeOfType<ItemRequest>();
            request.Item.ShouldBe(expectedItem);
            request.Amount.ShouldBe(expectedAmount);
            return true;

        }
    }
}
