using Xenko.Engine;

namespace CityBuilding.Linux
{
    internal class CityBuildingApp
    {
        private static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
