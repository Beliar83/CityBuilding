using System;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using GameEngine.EntityComponentSystem;
using JetBrains.Annotations;

namespace MessagingSystem.Akka
{
    internal class Messenger : MessagingSystem.Messenger
    {
        [NotNull] [ItemNotNull] private readonly EntityCollection entities;
        private static readonly int maxNumberOfNodes = 100;
        [NotNull] private readonly IActorRef shardRegion;

        public Messenger([NotNull] [ItemNotNull] EntityCollection entities,
            [NotNull] ActorSystem actorSystem)
        {
            this.entities = entities;

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
                    return EntityActor.GetProps(entity);
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
    }
}
