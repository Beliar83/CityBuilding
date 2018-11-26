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

        [NotNull] private readonly Dictionary<string, List<ReceiveDefinition>> receiveDefinitions =
            new Dictionary<string, List<ReceiveDefinition>>();

        [NotNull] private Entity entity;

        [Fact]
        public void EntityActorChangesState()
        {
            GivenDummyEntityExists();
            GivenEntityActorExists();
            receiveDefinitions["test"] = new List<ReceiveDefinition>
            {
                new ReceiveDefinition(
                    typeof(string),
                    message => { TestActor.Tell("Successful", ActorRefs.NoSender); },
                    message => true
                )
            };
            entityActor.Tell(new ChangeState("test"));
            ExpectNoMsg(1000);
            entityActor.Tell("Test");
            ExpectMsg<string>(s => s == "Successful");
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
    }
}
