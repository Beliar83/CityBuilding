using Akka.Actor;

namespace CityBuilding.Actors
{
    public class EntityManager : ReceiveActor
    {
        public static Props Props => Props.Create<EntityManager>();
    }
}
