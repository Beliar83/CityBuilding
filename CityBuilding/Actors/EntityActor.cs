using Akka.Actor;
using JetBrains.Annotations;
using Xenko.Engine;

namespace CityBuilding.Actors
{
    public class EntityActor : ReceiveActor
    {
        [NotNull] private readonly Entity entity;

        // ReSharper disable once MemberCanBePrivate.Global
        // Akka needs to have the constructor public
        public EntityActor([NotNull] Entity entity)
        {
            this.entity = entity;
            Receive<string>(_ => Sender.Tell(entity.Id));
        }

        [NotNull]
        public static Props GetProps([NotNull] Entity entity)
        {
            Props props = Props.Create(() => new EntityActor(entity));
            return props;
        }
    }
}
