﻿using Akka.Cluster.Sharding;
using JetBrains.Annotations;
using MessagingSystem.Messages;

namespace MessagingSystem.Akka
{
    public class EntityMessageExtractor : HashCodeMessageExtractor
    {
        /// <inheritdoc />
        public EntityMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards) { }

        /// <inheritdoc />
        [CanBeNull]
        public override string EntityId([NotNull] object message)
        {
            return (message as EntityMessageEnvelope)?.EntityId.ToString();
        }

        /// <inheritdoc />
        [CanBeNull]
        public override object EntityMessage([NotNull] object message)
        {
            return (message as EntityMessageEnvelope)?.Message;
        }
    }
}