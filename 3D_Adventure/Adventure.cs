using System;
using System.Text;

namespace Adventure
{
    class Adventure
    {
        private static MapDrawer game = new MapDrawer();

        static void Main(string[] args)
        {
            ScreenOptions.ConfigureConsole();

            game.Run().Wait();
        }
    }
}