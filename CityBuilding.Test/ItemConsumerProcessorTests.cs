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
    }
}
