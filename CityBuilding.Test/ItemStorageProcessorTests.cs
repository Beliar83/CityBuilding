using Akka.Actor;
using CityBuilding.Components;
using CityBuilding.Items;
using CityBuilding.Messages;
using CityBuilding.Processors;
using Moq.Protected;
using Xenko.Games;
using Xunit;

namespace CityBuilding.Test
{
    public class ItemStorageProcessorTests : TestWithMessenger
    {
        [Theory]
        [InlineData(0, 5, 5)]
        [InlineData(3, 5, 2)]
        [InlineData(5, 15, 10)]
        public void ProcessorRequestsItemsIfRequestThresholdNotMet(int current, int requestUntil, int expected)
        {
            GivenSceneExists();
            GivenMessengerExists();
            MessengerMock.Protected()
                .Setup<IActorRef>("CreateEntityManager")
                .Returns(() => TestActor);
            var itemStorageProcessor = new ItemStorageProcessor(Messenger);
            Game.SceneSystem.SceneInstance.Processors.Add(itemStorageProcessor);
            GivenTestEntityExists();
            GivenTestEntityIsInList();
            var itemStorage = new ItemStorage
            {
                Capacity = 10,
                Items =
                {
                    ["Test"] = new StoredItemData
                    {
                        CurrentCount = current,
                        MaxCount = requestUntil,
                        RequestUntil = requestUntil
                    }
                }
            };
            TestEntity.Components.Add(itemStorage);
            Game.SceneSystem.Update(new GameTime());
            ExpectMsg<CreateWalkerWithMessage>(
                message => WalkerTests.IsWalkerWithItemRequest(message, "Test", expected));
        }

        [Theory]
        [InlineData(10, 5)]
        [InlineData(5, 5)]
        [InlineData(30, 15)]
        public void ProcessorRequestsNoItemsIfAboveRequestThreshold(int current, int requestUntil)
        {
            GivenSceneExists();
            GivenMessengerExists();
            MessengerMock.Protected()
                .Setup<IActorRef>("CreateEntityManager")
                .Returns(() => TestActor);
            var itemStorageProcessor = new ItemStorageProcessor(Messenger);
            Game.SceneSystem.SceneInstance.Processors.Add(itemStorageProcessor);
            GivenTestEntityExists();
            GivenTestEntityIsInList();
            var itemStorage = new ItemStorage
            {
                Capacity = 10,
                Items =
                {
                    ["Test"] = new StoredItemData
                    {
                        CurrentCount = current,
                        MaxCount = requestUntil,
                        RequestUntil = requestUntil
                    }
                }
            };
            TestEntity.Components.Add(itemStorage);
            Game.SceneSystem.Update(new GameTime());
            ExpectNoMsg(1000);
        }

        [Fact]
        public void ProcessorRequestsNoItemsIfNoRequestSet()
        {
            GivenSceneExists();
            GivenMessengerExists();
            MessengerMock.Protected()
                .Setup<IActorRef>("CreateEntityManager")
                .Returns(() => TestActor);
            var itemStorageProcessor = new ItemStorageProcessor(Messenger);
            Game.SceneSystem.SceneInstance.Processors.Add(itemStorageProcessor);
            GivenTestEntityExists();
            GivenTestEntityIsInList();
            var itemStorage = new ItemStorage
            {
                Capacity = 10,
                Items = {["Test"] = new StoredItemData()}
            };
            TestEntity.Components.Add(itemStorage);
            Game.SceneSystem.Update(new GameTime());
            ExpectNoMsg(1000);
        }
    }
}
