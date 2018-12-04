using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;
using MessagingSystem.Akka.Exceptions;
using AkkaAddress = Akka.Actor.Address;

namespace MessagingSystem.Akka
{
    public class Messenger : MessagingSystem.Messenger, Receiver
    {
        [NotNull] [ItemNotNull] private readonly EntityCollection entities;
        private const int MaxNumberOfNodes = 100; // Just a random number for now.
        [NotNull] private readonly IActorRef shardRegion;
        [NotNull] private readonly Dictionary<string, List<ReceiveDefinition>> receiveDefinitions;
        [NotNull] private readonly Cluster cluster;

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
            cluster = Cluster.Get(actorSystem);
            ClusterSharding sharding = ClusterSharding.Get(actorSystem);
            shardRegion = sharding.Start(
                nameof(EntityActor),
                id =>
                {
                    // ReSharper disable once SuggestVarOrType_SimpleTypes
                    var entityId = Guid.Parse(id);
                    Entity entity = entities.GetEntityById(entityId);
                    if (entity is null)
                    {
                        throw new EntityNotFound(entityId);
                    }

                    return EntityActor.GetProps(entity, receiveDefinitions);
                },
                ClusterShardingSettings.Create(actorSystem),
                new EntityMessageExtractor(MaxNumberOfNodes * 10)
            );
        }

        /// <inheritdoc />
        public void SendMessage(object message)
        {
            shardRegion.Tell(message);
        }

        /// <inheritdoc />
        public bool Connect(string address)
        {
            try
            {
                cluster.Join(AkkaAddress.Parse(address));
                return true;
            }
            catch (UriFormatException exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        /// <inheritdoc />
        public string Address => cluster.SelfAddress.ToString();

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
