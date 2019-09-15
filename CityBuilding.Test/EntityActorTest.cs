using Akka.Actor;
using Akka.TestKit.Xunit2;
using CityBuilding.Actors;
using JetBrains.Annotations;
using Moq;
using Xenko.Engine;

// ReSharper disable NotNullMemberIsNotInitialized

namespace CityBuilding.Test
{
    public class TestEntityActor : TestKit
    {
        [NotNull] private IActorRef entityActor;

        [NotNull] private Entity entity;

        private void GivenDummyEntityExists()
        {
            var entityMock = new Mock<Entity>();
            entity = entityMock.Object;
        }

        private void GivenEntityActorExists()
        {
            entityActor = Sys.ActorOf(EntityActor.GetProps(entity));
        }
    }
}
