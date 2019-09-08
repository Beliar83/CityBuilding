using System;
using System.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using CityBuilding.Actors;
using CityBuilding.Exceptions;
using JetBrains.Annotations;
using Xenko.Engine;
using AkkaAddress = Akka.Actor.Address;

namespace CityBuilding
{
    public class Messenger
    {
        [NotNull] [ItemNotNull] private readonly Scene scene;
        private const int MaxNumberOfNodes = 100; // Just a random number for now.
        [NotNull] private readonly IActorRef shardRegion;
        [NotNull] private readonly Cluster cluster;

        public Messenger([NotNull] [ItemNotNull] Scene scene,
            [NotNull] ActorSystem actorSystem)
        {
            this.scene = scene;

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
                    Entity entity = scene.Entities.SingleOrDefault(e => e.Id.Equals(entityId));
                    if (entity is null)
                    {
                        throw new EntityNotFound(entityId);
                    }

                    return EntityActor.GetProps(entity);
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
    }
}
