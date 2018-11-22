using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;

namespace MessagingSystem.Akka
{
    internal class Messenger : MessagingSystem.Messenger, Receiver
    {
        [NotNull] [ItemNotNull] private readonly EntityCollection entities;
        private static readonly int maxNumberOfNodes = 100;
        [NotNull] private readonly IActorRef shardRegion;
        [NotNull] private readonly Dictionary<string, List<ReceiveDefinition>> receiveDefinitions;

        public Messenger([NotNull] [ItemNotNull] EntityCollection entities,
            [NotNull] ActorSystem actorSystem)
        {
            this.entities = entities;
            receiveDefinitions = new Dictionary<string, List<ReceiveDefinition>>();

            Config config = ConfigurationFactory.ParseString(@"
            akka.persistence{
              journal {
                    plugin = ""akka.persistence.journal.sqlite""
                    sqlite.connection-string = ""Datasource=journal;Mode=Memory;Cache=Shared""
                }
              } 
              snapshot-store{
                    plugin = ""akka.persistence.snapshot-store.sqlite""
                    sqlite.connection-string = ""Datasource=snapshot;Mode=Memory;Cache=Shared""
              }
            }
            ");

            ClusterSharding sharding = ClusterSharding.Get(actorSystem);
            shardRegion = sharding.Start(
                nameof(EntityActor),
                id =>
                {
                    Entity entity = entities.GetEntityById(Guid.Parse(id));
                    return EntityActor.GetProps(entity, receiveDefinitions);
                },
                ClusterShardingSettings.Create(actorSystem),
                new EntityMessageExtractor(maxNumberOfNodes * 10)
            );
        }

        /// <inheritdoc />
        public void SendMessage(object message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetupReceive(ReceiveDefinition receiveDefinition)
        {
            SetupReceive("Default", receiveDefinition);
        }

        /// <inheritdoc />
        public void SetupReceive(string state, ReceiveDefinition receiveDefinition)
        {
            if (!receiveDefinitions.ContainsKey(state))
            {
                receiveDefinitions[state] = new List<ReceiveDefinition>();
            }

            receiveDefinitions[state].Add(receiveDefinition);
        }
    }
}
