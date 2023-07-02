using Microsoft.VisualBasic;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Adventure
{
    /*
         *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*
        
          * The MapDrawer class is a program component responsible for drawing a map and controlling a game character.
          * 
          * Run() method: This is the main program loop that updates the screen and renders the 3D scene based on
          * the ray traced data.Inside this method, the user input is processed, ray tracing is done for each pixel on the screen,
          * and _screen is updated with information about the pixels to draw. Status information is also displayed and a mini-map is drawn.
          * 
          * CastRay(int x) method: This method performs ray tracing for each pixel along the horizontal axis of the screen and returns
          * information about the pixels that need to be rendered to create a 3D scene.Inside the method, the ray angle is calculated, the
          * intersection with obstacles on the map is checked, the shadow of the wall is determined, and the pixel
          * coordinates are calculated for drawing walls and floors.
          * 
          * Controller() method: It is responsible for handling user input and controlling the game character. Inside the method,
          * actions are determined based on the keys pressed, such as turning left or right, moving forward or backward.
          * The method also updates the map on each action to reflect changes in the player's position.
          * 
          * InitMap() method: It reads a map from a text document and initializes the _map internal variable.
          * The map is a text representation of the obstacles and the floor.
          * 
          * Methods OutStatus() and OutMiniMap(): They are responsible for displaying information about the player's status and
          * drawing the mini-map on _screen. The OutStatus() method outputs the player's position coordinates and the number of frames per second,
          * and the OutMiniMap() method copies the symbols from _map to the minimap in _screen and displays the player's symbol on the minimap.
          * 
          * In general, the MapDrawer class provides functionality for drawing a 3D scene and controlling a game character based on ray tracing and map data.
        
         *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-**-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*
    */ 

    public class MapDrawer
    {
        private const int _map_width = 30;
        private const int _map_height = 11;
        private const double _fov = Math.PI / 3;
        private const double _depth = 16;
        private static double _playerX = 2;
        private static double _playerY = 2;
        private static double _playerA = 0;
        private static char[] _screen = new char[ScreenOptions.Width * ScreenOptions.Height];
        private static readonly StringBuilder _map = new StringBuilder();
        private double elapsed_time;

        public async Task Run()  // Represents the main program loop that continuously updates the screen and renders a
        {                       // 3D scene based on ray-traced data.
            DateTime date_time_from = DateTime.Now;

            InitMap();

            while (true)
            {
                DateTime date_time_to = DateTime.Now;

                elapsed_time = (date_time_to - date_time_from).TotalSeconds;
                date_time_from = DateTime.Now;

                Controller();

                List<Task<Dictionary<int, char>>> ray_casting_task = new List<Task<Dictionary<int, char>>>();

                for (int x = 0; x < ScreenOptions.Width; x++)
                {
                    int x1 = x;

                    ray_casting_task.Add(Task.Run(() => CastRay(x1)));
                }

                foreach (Dictionary<int, char> dictionary in await Task.WhenAll(ray_casting_task))
                {
                    foreach (int key in dictionary.Keys)
                        _screen[key] = dictionary[key];
                }

                OutStatus();
                OutMiniMap();

                Console.SetCursorPosition(0, 0);
                Console.Write(_screen);
            }
        }

        private static Dictionary<int, char> CastRay(int x)  // The CastRay method performs a ray tracing for each pixel along the horizontal axis of 
        {                                                   // the screen and returns information about the pixels that need to be rendered to render the 3D scene.
            var result = new Dictionary<int, char>();
            double ray_angle = _playerA + _fov / 2 - x * _fov / ScreenOptions.Width;
            double rayX = Math.Sin(ray_angle);
            double rayY = Math.Cos(ray_angle);
            double distance_to_wall = 0;
            bool hit_wall = false;
            bool is_bounds = false;

            while (!hit_wall && distance_to_wall < _depth)
            {
                distance_to_wall += 0.1;

                int testX = (int)(_playerX + rayX * distance_to_wall);
                int testY = (int)(_playerY + rayY * distance_to_wall);

                if (testX < 0 || testX > _depth + _playerX || testY < 0 || testY > _depth + _playerY)
                {
                    hit_wall = true;
                    distance_to_wall = _depth;
                }
                else
                {
                    char test_cell = _map[testY * _map_width + testX];

                    if (test_cell == (char)Decor.wall)
                    {
                        hit_wall = true;

                        var bounds_vector_list = new List<(double module, double cos)>();

                        for (int tx = 0; tx < 2; tx++)
                        {
                            for (int ty = 0; ty < 2; ty++)
                            {
                                double vx = testX + tx - _playerX;
                                double vy = testY + ty - _playerY;
                                double vector_module = Math.Sqrt(vx * vx + vy * vy);
                                double cos_angle = rayX * vx / vector_module + rayY * vy / vector_module;

                                bounds_vector_list.Add((vector_module, cos_angle));
                            }
                        }
                        bounds_vector_list = bounds_vector_list.OrderBy(v => v.module).ToList();

                        double bound_angle = 0.03 / distance_to_wall;

                        if (Math.Acos(bounds_vector_list[0].cos) < bound_angle || Math.Acos(bounds_vector_list[1].cos) < bound_angle)
                            is_bounds = true;
                    }
                    else
                        _map[testY * _map_width + testX] = (char)Decor.floor;
                }
            }

            int ceiling = (int)(ScreenOptions.Height / 2d - ScreenOptions.Height * _fov / distance_to_wall);
            int floor = ScreenOptions.Height - ceiling;
            char wall_shade;

            if (is_bounds)
                wall_shade = (char)Decor.bound;
            else if (distance_to_wall <= _depth / 4d)
                wall_shade = (char)Decor.wall_4d;
            else if (distance_to_wall < _depth / 3d)
                wall_shade = (char)Decor.wall_3d;
            else if (distance_to_wall < _depth / 2d)
                wall_shade = (char)Decor.wall_2d;
            else if (distance_to_wall < _depth)
                wall_shade = (char)Decor.wall_d;
            else
                wall_shade = (char)Decor.space;

            for (int y = 0; y < ScreenOptions.Height; y++)
            {
                if (y <= ceiling)
                    result[y * ScreenOptions.Width + x] = (char)Decor.space;
                else if (y > ceiling && y <= floor)
                    result[y * ScreenOptions.Width + x] = wall_shade;
                else
                {
                    char floor_shade;
                    double b = 1 - (y - ScreenOptions.Height / 2d) / (ScreenOptions.Height / 2d);

                    if (b < 0.5)
                        floor_shade = (char)Decor.wall_2d;
                    else
                        floor_shade = (char)Decor.floor;

                    result[y * ScreenOptions.Width + x] = floor_shade;
                }
            }
            return result;
        }

        private void Controller()  // Is responsible for processing user input and managing the game character.
        {
            if (Console.KeyAvailable)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case (ConsoleKey)Control.left:
                        _playerA += elapsed_time * 5;
                        break;
                    case (ConsoleKey)Control.right:
                        _playerA -= elapsed_time * 5;
                        break;
                    case (ConsoleKey)Control.forward:
                        {
                            _playerX += Math.Sin(_playerA) * 20 * elapsed_time;
                            _playerY += Math.Cos(_playerA) * 20 * elapsed_time;

                            if (_map[(int)_playerY * _map_width + (int)_playerX] == (char)Decor.wall)
                            {
                                _playerX -= Math.Sin(_playerA) * 20 * elapsed_time;
                                _playerY -= Math.Cos(_playerA) * 20 * elapsed_time;
                            }
                            break;
                        }
                    case (ConsoleKey)Control.back:
                        {
                            _playerX -= Math.Sin(_playerA) * 20 * elapsed_time;
                            _playerY -= Math.Cos(_playerA) * 20 * elapsed_time;

                            if (_map[(int)_playerY * _map_width + (int)_playerX] == (char)Decor.wall)
                            {
                                _playerX += Math.Sin(_playerA) * 20 * elapsed_time;
                                _playerY += Math.Cos(_playerA) * 20 * elapsed_time;
                            }
                            break;
                        }
                    default:
                        break;
                }

                InitMap();
            }
        }

        private static void InitMap()  // Reads a map from a text document.
        {
            string[] map = File.ReadAllLines("MAP.txt");

            _map.Clear();
            foreach (string line in map)
                _map.Append(line);
        }

        private void OutStatus()  // Shows the coordinates of the player's position, and the number and number of frames.
        {
            char[] status = $"X: {_playerX}  Y: {_playerY}  A: {_playerA}  FPS: {(int)(1 / elapsed_time)}".ToCharArray();

            status.CopyTo(_screen, 0);
        }

        private void OutMiniMap()  // Сopies the symbols from the map to the mini-map into the 
        {                         // _screen array and displays the player's symbol on the mini-map.
            for (int x = 0; x < _map_width; x++)
            {
                for (int y = 0; y < _map_height; y++)
                    _screen[(y + 1) * ScreenOptions.Width + x] = _map[y * _map_width + x];
            }

            _screen[(int)(_playerY + 1) * ScreenOptions.Width + (int)_playerX] = (char)15;
        }
    }
}
