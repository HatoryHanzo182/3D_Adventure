using System;
using System.Text;

namespace Adventure
{
    class Adventure
    {
        private static MapDrawer map = new MapDrawer();

        static void Main(string[] args)
        {
            ScreenOptions.ConfigureConsole();

            map.VOV().Wait();
        }
    }
}