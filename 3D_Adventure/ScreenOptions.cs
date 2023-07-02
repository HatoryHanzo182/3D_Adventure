using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    public class ScreenOptions
    {
        private static int _screen_width;
        private static int _screen_height;

        static ScreenOptions()
        {
            _screen_width = 200;
            _screen_height = 40;
        }

        public static int Width { get => _screen_width; }
        public static int Height { get => _screen_height; }

        public static void ConfigureConsole()
        {
            Console.SetWindowSize(ScreenOptions.Width, ScreenOptions.Height);
            Console.SetBufferSize(ScreenOptions.Width, ScreenOptions.Height);
            Console.CursorVisible = false;
        }
    }
}
