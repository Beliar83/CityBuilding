using Xenko.Engine;

namespace CityBuilding.Windows
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
