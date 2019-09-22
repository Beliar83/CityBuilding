using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using CityBuilding.Components;
using CityBuilding.Items;
using CityBuilding.Messages;
using CityBuilding.Processors;
using Xenko.Games;
using Xunit;
using Moq.Protected;
using Shouldly;
using Xenko.Engine.Network;
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
        
        [Theory]
        [MemberData( nameof(GetItemConsumerSubtractsItemsData))]
        public void ItemConsumerSubtractsItems(
            int seconds, 
            Dictionary<string, NeededItemData> neededItems, 
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
            TestEntity.Components.Add(itemConsumer);
            var elapsedTime = new TimeSpan(0,0,0, seconds);
            Game.SceneSystem.Update(new GameTime(elapsedTime, elapsedTime));
            foreach ((string key, int expectedCount) in expectedCounts)
            {
                itemConsumer.NeededItems[key].CurrentCount.ShouldBe(expectedCount);
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
            return testData.Select(d => new object[]{d.Seconds, d.NeededItems, d.ExpectedCounts});
        }

        private struct ItemConsumerSubtractsItemsData
        {
            public int Seconds;
            public Dictionary<string, NeededItemData> NeededItems;
            public Dictionary<string, int> ExpectedCounts;
        }
    }
}
