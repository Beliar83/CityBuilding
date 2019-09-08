using Xenko.Engine;

namespace CityBuilding.Linux
{
    class CityBuildingApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
