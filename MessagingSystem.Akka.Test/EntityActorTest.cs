using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;
using MessagingSystem.Messages;
using Moq;
using Xunit;

// ReSharper disable NotNullMemberIsNotInitialized

namespace MessagingSystem.Akka.Test
{
    public class TestEntityActor : TestKit
    {
        [NotNull] private IActorRef entityActor;
        [NotNull] private Dictionary<string, List<ReceiveDefinition>> receiveDefinitions;
        [NotNull] private Entity entity;

        [Fact]
        public void TestChangeState()
        {
            GivenDummyEntityExists();
            receiveDefinitions = new Dictionary<string, List<ReceiveDefinition>>
            {
                ["test"] = new List<ReceiveDefinition>
                {
                    new ReceiveDefinition(
                        typeof(string),
                        message => { TestActor.Tell("Successful", ActorRefs.NoSender); },
                        message => true
                    )
                }
            };
            GivenEntityActorExists();
            entityActor.Tell(new ChangeState("test"));
            ExpectNoMsg(1000);
            entityActor.Tell("Test");
            ExpectMsg<string>();
        }

        private void GivenDummyEntityExists()
        {
            var entityMock = new Mock<Entity>();
            entity = entityMock.Object;
        }

        private void GivenEntityActorExists()
        {
            entityActor = Sys.ActorOf(EntityActor.GetProps(entity, receiveDefinitions));
        }


        [ItemNotNull]
        [NotNull]
        private static List<Entity> CreateEntityList(int number)
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
