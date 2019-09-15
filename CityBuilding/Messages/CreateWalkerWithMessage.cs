namespace CityBuilding.Messages
{
    public struct CreateWalkerWithMessage
    {
        public object Message { get; }

        public CreateWalkerWithMessage(object message)
        {
            Message = message;
        }
    }
}
