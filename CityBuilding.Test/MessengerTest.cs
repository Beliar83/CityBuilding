using System;
using Akka.Actor;
using CityBuilding.Exceptions;
using CityBuilding.Messages;
using Moq.Protected;
using Xunit;

namespace CityBuilding.Test
{
    public class MessengerTest : TestWithMessenger
    {
        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void SendMessageToEntitySendsToEntity(int entityCount)
        {
            GivenSceneExists();
            GivenEntityListContainsEntities(entityCount);
            GivenTestEntityExists();
            GivenTestEntityIsInList();
            GivenMessengerExists();
            GivenMessengerIsConnectedToItself();
            Messenger.SendMessageToEntity(new EntityMessageEnvelope(TestEntity.Id, "Test"));
            ExpectMsg<Guid>(msg => msg == TestEntity.Id, new TimeSpan(0, 0, 0, 10));
        }

        [Fact]
        public void MessengerThrowsOnWrongEntityId()
        {
            GivenSceneExists();
            GivenMessengerExists();
            GivenMessengerIsConnectedToItself();
            GivenTestEntityExists();
            EventFilter.Exception<EntityNotFound>().ExpectOne(() =>
                Messenger.SendMessageToEntity(new EntityMessageEnvelope(TestEntity.Id, "Test")));
        }

        [Fact]
        public void MessengerReturnsTrueOnCorrectUri()
        {
            GivenSceneExists();
            GivenMessengerExists();
            Assert.True(Messenger.Connect(Messenger.Address));
        }

        [Fact]
        public void MessengerReturnsFalseOnWrongUri()
        {
            GivenSceneExists();
            GivenMessengerExists();
            Assert.False(Messenger.Connect(string.Empty));
        }

        private void GivenMessengerIsConnectedToItself()
        {
            Messenger.Connect(Messenger.Address);
        }

        [Fact]
        public void SendMessageToEntityManagerSendsToEntityManager()
        {
            GivenSceneExists();
            GivenMessengerExists();
            MessengerMock.Protected()
                .Setup<IActorRef>("CreateEntityManager")
                .Returns(() => TestActor);
            Messenger.SendMessageToEntityManager("Test");
            ExpectMsg("Test");
        }
    }
}
