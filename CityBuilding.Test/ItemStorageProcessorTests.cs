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

        [Theory]
        [InlineData(10, 5, 5)]
        [InlineData(6, 5, 1)]
        [InlineData(50, 30, 20)]
        public void ProcessorAsksForStorageIfAboveEmptyThreshold(int current, int emptyUntil, int expected)
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
                Capacity = current,
                Items =
                {
                    ["Test"] = new StoredItemData
                    {
                        CurrentCount = current,
                        MaxCount = current,
                        EmptyUntil = emptyUntil
                    }
                }
            };
            TestEntity.Components.Add(itemStorage);
            Game.SceneSystem.Update(new GameTime());
            ExpectMsg<CreateWalkerWithMessage>(
                message => WalkerTests.IsWalkerWithStorageRequest(message, "Test", expected));
        }

        [Theory]
        [InlineData(2, 5)]
        [InlineData(10, 10)]
        [InlineData(28, 30)]
        public void ProcessorRequestsStorageIfBelowEmptyThreshold(int current, int emptyUntil)
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
                        MaxCount = current,
                        EmptyUntil = emptyUntil
                    }
                }
            };
            TestEntity.Components.Add(itemStorage);
            Game.SceneSystem.Update(new GameTime());
            ExpectNoMsg(1000);
        }

        [Fact]
        public void ProcessorRequestsNoStorageIfEmptyUntilNotSet()
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
