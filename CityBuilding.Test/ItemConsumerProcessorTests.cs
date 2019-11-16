using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;
using CityBuilding.Components;
using CityBuilding.Items;
using CityBuilding.Messages;
using CityBuilding.Processors;
using Moq.Protected;
using Shouldly;
using Xenko.Games;
using Xunit;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CityBuilding.Test
{
    public class ItemConsumerProcessorTests : TestWithMessenger
    {
        [Theory]
        [InlineData("Test", 1, 10, 5, 9)]
        [InlineData("Test", 4, 20, 5, 16)]
        public void ItemConsumerAsksForWalkerWithItemRequestWhenBelowThreshold(string item, int currentCount,
            int maxCount, int orderThreshold, int expectedAmount)
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
                        {OrderThreshold = orderThreshold}
                }
            };

            var itemStorage = new ItemStorage
            {
                Capacity = maxCount,
                Items =
                {
                    [item] = new StoredItemData
                    {
                        CurrentCount = currentCount,
                        MaxCount = maxCount
                    }
                }
            };
            TestEntity.Components.Add(itemStorage);
            TestEntity.Components.Add(itemConsumer);
            Game.SceneSystem.Update(new GameTime());
            ExpectMsg<CreateWalkerWithMessage>(message =>
                WalkerTests.IsWalkerWithItemRequest(message, item, expectedAmount));
        }

        [Theory]
        [MemberData(nameof(GetItemConsumerSubtractsItemsData))]
        public void ItemConsumerSubtractsItems(
            int seconds,
            Dictionary<string, NeededItemData> neededItems,
            Dictionary<string, StoredItemData> storedItems,
            Dictionary<string, int> expectedCounts)
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
            var itemConsumer = new ItemConsumer(neededItems);
            var itemStorage = new ItemStorage(storedItems);
            TestEntity.Components.Add(itemStorage);
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0, 0, 0, seconds);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            foreach ((string key, int expectedCount) in expectedCounts)
            {
                itemStorage.Items[key].CurrentCount.ShouldBe(expectedCount);
            }
        }

        public static IEnumerable<object[]> GetItemConsumerSubtractsItemsData()
        {
            var yamlStream = new YamlStream();
            string yamlData = File.ReadAllText($"{nameof(ItemConsumerSubtractsItems)}Data.yaml");
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            var testData = deserializer.Deserialize<List<ItemConsumerSubtractsItemsData>>(yamlData);
            return testData.Select(d => new object[] {d.Seconds, d.NeededItems, d.StoredItems, d.ExpectedCounts});
        }

        private struct ItemConsumerSubtractsItemsData
        {
#pragma warning disable 649 // It is set from the deserializer
            public int Seconds;
            public Dictionary<string, NeededItemData> NeededItems;
            public Dictionary<string, StoredItemData> StoredItems;
            public Dictionary<string, int> ExpectedCounts;
#pragma warning restore 649
        }

        [Fact]
        public void ProcessorSetsActiveToTrueIfItemsWereConsumed()
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
            const string itemName = "Test";
            const int currentCount = 1;
            var itemStorage = new ItemStorage
            {
                Items =
                {
                    [itemName] = new StoredItemData
                    {
                        CurrentCount = currentCount
                    }
                }
            };
            var itemConsumer = new ItemConsumer
            {
                NeededItems =
                {
                    [itemName] = new NeededItemData
                    {
                        ConsumptionPerSecond = currentCount
                    }
                }
            };
            var activatable = new Activatable
            {
                Active = false
            };
            TestEntity.Components.Add(itemStorage);
            TestEntity.Components.Add(activatable);
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0, 0, 0, 1);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            activatable.Active.ShouldBe(true);
        }

        [Fact]
        public void ProcessorSetsActiveToFalseIfItemsWereNotConsumed()
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
            const string itemName = "Test";
            const int currentCount = 1;
            var itemStorage = new ItemStorage
            {
                Items =
                {
                    [itemName] = new StoredItemData
                    {
                        CurrentCount = currentCount
                    }
                }
            };
            var itemConsumer = new ItemConsumer
            {
                NeededItems =
                {
                    [itemName] = new NeededItemData
                    {
                        ConsumptionPerSecond = currentCount + 1
                    }
                }
            };
            var activatable = new Activatable
            {
                Active = true
            };
            TestEntity.Components.Add(itemStorage);
            TestEntity.Components.Add(activatable);
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0, 0, 0, 1);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            activatable.Active.ShouldBe(false);
        }

        [Fact]
        public void ProcessorCreatesAssociatedComponentsIfNotPresent()
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
            var itemConsumer = new ItemConsumer();
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0, 0, 0, 5);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            TestEntity.Components.Get<ItemStorage>().ShouldNotBeNull();
            TestEntity.Components.Get<Activatable>().ShouldNotBeNull();
        }

        [Fact]
        public void ProcessorSetsStorageToBeAutomaticallyControlled()
        {
            GivenSceneExists();
            GivenMessengerExists();
            var itemConsumerProcessor = new ItemConsumerProcessor(Messenger);
            Game.SceneSystem.SceneInstance.Processors.Add(itemConsumerProcessor);
            GivenTestEntityExists();
            GivenTestEntityIsInList();
            var itemStorage = new ItemStorage
            {
                AutomaticallyControlled = false
            };
            TestEntity.Components.Add(itemStorage);
            var itemConsumer = new ItemConsumer();
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0, 0, 0, 5);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            itemStorage.AutomaticallyControlled.ShouldBeTrue();
        }

        [Fact]
        public void ProcessorAddsStorageEntryForMissingItems()
        {
            GivenSceneExists();
            GivenMessengerExists();
            var itemConsumerProcessor = new ItemConsumerProcessor(Messenger);
            Game.SceneSystem.SceneInstance.Processors.Add(itemConsumerProcessor);
            GivenTestEntityExists();
            GivenTestEntityIsInList();
            var itemStorage = new ItemStorage();
            const string itemName = "Test";
            var itemConsumer = new ItemConsumer
            {
                NeededItems = {[itemName] = new NeededItemData()}
            };

            TestEntity.Components.Add(itemStorage);
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0, 0, 0, 5);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            itemStorage.Items.ShouldContainKey(itemName);
        }

        [Fact]
        public void ProcessorDoesNotRequestItemsWhenAboveThreshold()
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
            const string itemName = "Test";
            var itemStorage = new ItemStorage
            {
                Items =
                {
                    [itemName] = new StoredItemData
                    {
                        CurrentCount = 5
                    }
                }
            };
            var itemConsumer = new ItemConsumer
            {
                NeededItems =
                {
                    [itemName] = new NeededItemData
                    {
                        OrderThreshold = 5
                    }
                }
            };

            TestEntity.Components.Add(itemStorage);
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0, 0, 0, 5);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            ExpectNoMsg(1000);
        }

        [Fact]
        public void ProcessorDoesNotSubtractItemsIfStorageIsLowerThanConsumption()
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
            const string itemName = "Test";
            const int currentCount = 1;
            var itemStorage = new ItemStorage
            {
                AutomaticallyControlled = false,
                Items =
                {
                    [itemName] = new StoredItemData
                    {
                        CurrentCount = currentCount
                    }
                }
            };
            var itemConsumer = new ItemConsumer
            {
                NeededItems =
                {
                    [itemName] = new NeededItemData
                    {
                        ConsumptionPerSecond = 2
                    }
                }
            };

            TestEntity.Components.Add(itemStorage);
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0, 0, 0, 5);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            itemStorage.Items[itemName].CurrentCount.ShouldBe(currentCount);
        }
    }
}
