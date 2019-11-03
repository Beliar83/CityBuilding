using System;
using System.Collections.Generic;
using Akka.TestKit.Xunit2;
using JetBrains.Annotations;
using Moq;
using Xenko.Core;
using Xenko.Engine;

namespace CityBuilding.Test
{
    public class TestWithMessenger : TestKit
    {
        [NotNull] protected Mock<Messenger> MessengerMock;
        [NotNull] protected Messenger Messenger => MessengerMock.Object;
        [NotNull] protected Game Game;
        [NotNull] protected Scene Scene => Game.SceneSystem.SceneInstance.RootScene;
        [NotNull] protected Entity TestEntity;

        public TestWithMessenger() : base(@"
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

        protected void GivenMessengerExists()
        {
            MessengerMock = new Mock<Messenger>(Scene, Sys) {CallBase = true};
        }

        protected void GivenSceneExists()
        {
            Game = new Game();
            Game.SceneSystem.SceneInstance = new SceneInstance(new ServiceRegistry(), new Scene());
        }

        [ItemNotNull]
        [NotNull]
        private static IEnumerable<Entity> CreateEntityList(int number)
        {
            var entities = new List<Entity>();
            for (var i = 0; i < number; i++)
            {
                entities.Add(new Entity {Id = Guid.NewGuid()});
            }

            return entities;
        }

        protected void GivenTestEntityIsInList()
        {
            var random = new Random();
            Scene.Entities.Insert(random.Next(Scene.Entities.Count), TestEntity);
        }

        protected void GivenEntityListContainsEntities(int entityCount)
        {
            GivenSceneExists();
            Scene.Entities.AddRange(CreateEntityList(entityCount));
        }

        protected void GivenTestEntityExists()
        {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            var testGuid = Guid.NewGuid();
            TestEntity = new Entity
            {
                Id = testGuid
            };
        }
    }
}
