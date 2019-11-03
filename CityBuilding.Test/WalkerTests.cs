using CityBuilding.Messages;
using Shouldly;

namespace CityBuilding.Test
{
    internal static class WalkerTests
    {
        public static bool IsWalkerWithItemRequest(CreateWalkerWithMessage message, string expectedItem,
            int expectedAmount)
        {
            var request = message.Message.ShouldBeOfType<ItemRequest>();
            request.Item.ShouldBe(expectedItem);
            request.Amount.ShouldBe(expectedAmount);
            return true;
        }

        public static bool IsWalkerWithStorageRequest(CreateWalkerWithMessage message, string expectedItem,
            int expectedAmount)
        {
            var request = message.Message.ShouldBeOfType<StorageQuery>();
            request.Item.ShouldBe(expectedItem);
            request.Amount.ShouldBe(expectedAmount);
            return true;
        }
    }
}
