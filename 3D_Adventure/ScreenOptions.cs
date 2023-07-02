using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    // This class provides options for configuring and controlling console window settings.
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

        public static void ConfigureConsole()  // The method is used to adjust the console window using the specified width and height parameters.
        {                                     // It calls the Console.SetWindowSize() and Console.SetBufferSize() methods to set the window and console buffer sizes, respectively.
                                             //  It then sets the value of the Console.CursorVisible property to false to hide the console cursor.
            Console.SetWindowSize(ScreenOptions.Width, ScreenOptions.Height);
            Console.SetBufferSize(ScreenOptions.Width, ScreenOptions.Height);
            Console.CursorVisible = false;
        }
    }
}
