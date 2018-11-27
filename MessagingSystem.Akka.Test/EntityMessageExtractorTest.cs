using System;
using MessagingSystem.Messages;
using Xunit;

namespace MessagingSystem.Akka.Test
{
    public class EntityMessageExtractorTest
    {
        private static readonly Guid TestId = Guid.NewGuid();
        private const string Message = "Test";

        [Fact]
        public void ExtractsValidMessageId()
        {
            var entityMessageEnvelope = new EntityMessageEnvelope(TestId, Message);
            var messageExtractor = new EntityMessageExtractor(1);
            Assert.Equal(TestId.ToString(), messageExtractor.EntityId(entityMessageEnvelope));
        }

        [Fact]
        public void ExtractsValidMessage()
        {
            var entityMessageEnvelope = new EntityMessageEnvelope(TestId, Message);
            var messageExtractor = new EntityMessageExtractor(1);
            Assert.Equal(Message, messageExtractor.EntityMessage(entityMessageEnvelope));
        }

        [Fact]
        public void ReturnsNullIdOnWrongEnvelope()
        {
            var invalidEnvelope = new InvalidEnvelope();
            var messageExtractor = new EntityMessageExtractor(1);
            Assert.Null(messageExtractor.EntityId(invalidEnvelope));
        }

        private class InvalidEnvelope { }

        [Fact]
        public void ReturnsNullMessageOnWrongEnvelope()
        {
            var invalidEnvelope = new InvalidEnvelope();
            var messageExtractor = new EntityMessageExtractor(1);
            Assert.Null(messageExtractor.EntityMessage(invalidEnvelope));
        }
    }
}
