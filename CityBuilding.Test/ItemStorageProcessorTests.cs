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
        [InlineData(5, 0, 5, 5, 5)]
        [InlineData(5, 3, 5, 5, 2)]
        [InlineData(15, 5, 15, 15, 10)]
        [InlineData(10, 5, 15, 15, 5)]
        [InlineData(15, 5, 15, 10, 5)]
        [InlineData(10, 0, 15, 15, 10)]
        [InlineData(null, 5, 10, null, 5)]
        [InlineData(null, 10, 20, 11, 1)]
        [InlineData(20, 15, 30, null, 5)]
        public void ProcessorRequestsItemsIfRequestThresholdNotMet(int? capacity, int current, int requestUntil,
            int? maxCount,
            int expected)
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
                Capacity = capacity,
                Items =
                {
                    ["Test"] = new StoredItemData
                    {
                        CurrentCount = current,
                        MaxCount = maxCount,
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
        public void ProcessorCutsOffLowPriorityItemsIfCapacityTooLow()
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
                Capacity = 25,
                Items =
                {
                    ["Test"] = new StoredItemData
                    {
                        CurrentCount = 5,
                        MaxCount = 15,
                        RequestUntil = 15
                    },
                    ["Test2"] = new StoredItemData
                    {
                        Priority = 1,
                        CurrentCount = 5,
                        MaxCount = 15,
                        RequestUntil = 15
                    }
                }
            };
            TestEntity.Components.Add(itemStorage);
            Game.SceneSystem.Update(new GameTime());
            ExpectMsg<CreateWalkerWithMessage>(
                message => WalkerTests.IsWalkerWithItemRequest(message, "Test2", 10));
            ExpectMsg<CreateWalkerWithMessage>(
                message => WalkerTests.IsWalkerWithItemRequest(message, "Test", 5));
            ExpectNoMsg();
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

        [Fact]
        public void ProcessorRequestsNoItemsIfCapacityReached()
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
                        RequestUntil = 15,
                        CurrentCount = 10
                    }
                }
            };
            TestEntity.Components.Add(itemStorage);
            Game.SceneSystem.Update(new GameTime());
            ExpectNoMsg(1000);
        }

        [Fact]
        public void ProcessorRequestsNoItemsIfMaxCountReached()
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
                Items =
                {
                    ["Test"] = new StoredItemData
                    {
                        RequestUntil = 15,
                        CurrentCount = 10,
                        MaxCount = 10
                    }
                }
            };
            TestEntity.Components.Add(itemStorage);
            Game.SceneSystem.Update(new GameTime());
            ExpectNoMsg(1000);
        }
    }
}
