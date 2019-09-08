using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using CityBuilding.Exceptions;
using CityBuilding.Messages;
using JetBrains.Annotations;
using Moq;
using Xenko.Core.Collections;
using Xenko.Engine;
using Xunit;

namespace CityBuilding.Test
{
    public class MessengerTest : TestKit
    {
        [NotNull] private Entity testEntity;
        [NotNull] private Messenger messenger;
        [NotNull] private Scene scene;

        // https://github.com/akkadotnet/akka.net/issues/3051
        // ReSharper disable once NotNullMemberIsNotInitialized
        public MessengerTest() : base(@"
        akka {
            actor {
                provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
            }
            persistence {
            {
                snapshot-store {
                    sharding {
                        plugin-dispatcher = ""akka.actor.default-dispatcher""
                        connection-string = ""<connection-string>""
                        connection-timeout = 30s
                        schema-name = dbo
                        table-name = ShardingSnapshotStore
                        auto-initialize = on
                    }
                }
            }
        }
        ") { }

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void MessengerSendsToEntity(int entityCount)
        {
            GivenEntityListContainsEntities(entityCount);
            GivenTestEntityExists();
            GivenTestEntityIsInList();
            GivenMessengerExists();
            GivenMessengerIsConnectedToItself();
            messenger.SendMessage(new EntityMessageEnvelope(testEntity.Id, "Test"));
            ExpectMsg<Guid>(msg => msg == testEntity.Id, new TimeSpan(0,0,0,10));
        }

        [Fact]
        public void MessengerThrowsOnWrongEntityId()
        {
            GivenMessengerExists();
            GivenMessengerIsConnectedToItself();
            GivenTestEntityExists();
            EventFilter.Exception<EntityNotFound>().ExpectOne(() =>
                messenger.SendMessage(new EntityMessageEnvelope(testEntity.Id, "Test")));
        }

        [Fact]
        public void MessengerReturnsTrueOnCorrectUri()
        {
            GivenMessengerExists();
            Assert.True(messenger.Connect(messenger.Address));
        }

        [Fact]
        public void MessengerReturnsFalseOnWrongUri()
        {
            GivenMessengerExists();
            Assert.False(messenger.Connect(string.Empty));
        }

        private void GivenMessengerIsConnectedToItself()
        {
            messenger.Connect(messenger.Address);
        }

        private void GivenMessengerExists()
        {
            GivenSceneExists();
            messenger = messenger ?? new Messenger(scene, Sys);
        }

        private void GivenSceneExists()
        {
            scene = scene ?? new Scene();
        }
        
        private void GivenTestEntityIsInList()
        {
            GivenSceneExists();
            var random = new Random();
            scene.Entities.Insert(random.Next(scene.Entities.Count), testEntity);
        }

        private void GivenEntityListContainsEntities(int entityCount)
        {
            GivenSceneExists();
            scene.Entities.AddRange(CreateEntityList(entityCount));
        }

        private void GivenTestEntityExists()
        {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            var testGuid = Guid.NewGuid();
            testEntity = new Entity()
            {
                Id = testGuid,
            };
        }

        [ItemNotNull]
        [NotNull]
        private static IEnumerable<Entity> CreateEntityList(int number)
        {
            var entities = new List<Entity>();
            for (var i = 0; i < number; i++)
            {
                entities.Add(new Entity() {Id = Guid.NewGuid()});
            }

            return entities;
        }
    }
}
