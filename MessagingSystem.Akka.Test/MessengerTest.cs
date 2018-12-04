using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;
using MessagingSystem.Messages;
using Moq;
using Xunit;

namespace MessagingSystem.Akka.Test
{
    public class MessengerTest : TestKit
    {
        [ItemNotNull] [NotNull] private readonly List<Entity> entities = new List<Entity>();
        [NotNull] private Entity testEntity;
        [NotNull] private Messenger messenger;

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
            GivenEntityTellsIdOnStringMessage();
            messenger.SendMessage(new EntityMessageEnvelope(testEntity.Id, new ChangeState("Default")));
            ExpectNoMsg(100);
            messenger.SendMessage(new EntityMessageEnvelope(testEntity.Id, "Test"));
            ExpectMsg<Guid>(msg => msg == testEntity.Id);
        }

        private void GivenMessengerIsConnectedToItself()
        {
            messenger.Connect(messenger.Address);
        }

        private void GivenEntityTellsIdOnStringMessage()
        {
            messenger.SetupReceive(new ReceiveDefinition(typeof(string),
                message => { TestActor.Tell(testEntity.Id, ActorRefs.NoSender); },
                message => true));
        }

        private void GivenMessengerExists()
        {
            messenger = new Messenger(CreateMockEntityCollection(entities), Sys);
        }

        private void GivenTestEntityIsInList()
        {
            var random = new Random();
            entities.Insert(random.Next(entities.Count), testEntity);
        }

        [NotNull]
        [ItemNotNull]
        private static EntityCollection CreateMockEntityCollection([NotNull] [ItemNotNull] IEnumerable<Entity> enumerable)
        {
            var mockEntities = new Mock<EntityCollection>();
            mockEntities.Setup(el => el.GetEnumerator()).Returns(enumerable.GetEnumerator);
            EntityCollection entityCollection = mockEntities.Object;
            return entityCollection;
        }

        private void GivenEntityListContainsEntities(int entityCount)
        {
            entities.AddRange(CreateEntityList(entityCount));
        }

        private void GivenTestEntityExists()
        {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            var testGuid = Guid.NewGuid();
            var mockEntity = new Mock<Entity>();
            mockEntity.Setup(e => e.Id).Returns(testGuid);
            testEntity = mockEntity.Object;
        }

        [ItemNotNull]
        [NotNull]
        private static IEnumerable<Entity> CreateEntityList(int number)
        {
            var entities = new List<Entity>();
            for (var i = 0; i < number; i++)
            {
                var entityMock = new Mock<Entity>();
                entityMock.Setup(m => m.Id).Returns(Guid.NewGuid());
                entities.Add(entityMock.Object);
            }

            return entities;
        }
    }
}
