using System;
using System.Text;

namespace Adventure
{
    class Program
    {
        private const int _screen_width = 200;
        private const int _screen_height = 40;

        private const int _map_width = 66;
        private const int _map_height = 28;

        private const double _fov = Math.PI / 3;
        private const double _depth = 16;

        private static double _playerX = 2;
        private static double _playerY = 2;
        private static double _playerA = 0;

        private static readonly char[] _screen = new char[_screen_width * _screen_height];
        private static readonly StringBuilder _map = new StringBuilder();

        static async Task Main(string[] args)
        {
            Console.SetWindowSize(_screen_width, _screen_height);
            Console.SetBufferSize(_screen_width, _screen_height);
            Console.CursorVisible = false;

            DateTime date_time_from = DateTime.Now;

            InitMap();

            while (true)
            {

                DateTime date_time_to = DateTime.Now;
                double elapsed_time = (date_time_to - date_time_from).TotalSeconds;

                date_time_from = DateTime.Now;

                if (Console.KeyAvailable)
                {
                    ConsoleKey console_key = Console.ReadKey(true).Key;

                    switch (console_key)
                    {
                        case ConsoleKey.A:
                            _playerA += elapsed_time * 2;
                            break;
                        case ConsoleKey.D:
                            _playerA -= elapsed_time * 2;
                            break;
                        case ConsoleKey.W:
                            {
                                _playerX += Math.Sin(_playerA) * 20 * elapsed_time;
                                _playerY += Math.Cos(_playerA) * 20 * elapsed_time;

                                if (_map[(int)_playerY * _map_width + (int)_playerX] == '#')
                                {
                                    _playerX -= Math.Sin(_playerA) * 20 * elapsed_time;
                                    _playerY -= Math.Cos(_playerA) * 20 * elapsed_time;
                                }
                                break;
                            }
                        case ConsoleKey.S:
                            {
                                _playerX -= Math.Sin(_playerA) * 20 * elapsed_time;
                                _playerY -= Math.Cos(_playerA) * 20 * elapsed_time;

                                if (_map[(int)_playerY * _map_width + (int)_playerX] == '#')
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

                var ray_casting_task = new List<Task<Dictionary<int, char>>>();

                for (int x = 0; x < _screen_width; x++)
                {
                    int x1 = x;

                    ray_casting_task.Add(Task.Run(() => CastRay(x1)));
                }

                var rays = await Task.WhenAll(ray_casting_task);

                foreach (Dictionary<int, char> dictionary in rays)
                {
                    foreach (int key in dictionary.Keys)
                    {
                        _screen[key] = dictionary[key];
                    }
                }

                // Status.
                char[] status = $"X: {_playerX}  Y: {_playerY}  A: {_playerA}  FPS: {(int)(1 / elapsed_time)}".ToCharArray();

                status.CopyTo(_screen, 0);


                // Mini map.
                for (int x = 0; x < _map_width; x++)
                {
                    for (int y = 0; y < _map_height; y++)
                    {
                        _screen[(y + 1) * _screen_width + x] = _map[y * _map_width + x];
                    }
                }

                _screen[(int)(_playerY + 1) * _screen_width + (int)_playerX] = '✯';


                Console.SetCursorPosition(0, 0);
                Console.Write(_screen);
            }
        }

        public static Dictionary<int, char> CastRay(int x)
        {
            var result = new Dictionary<int, char>();
            double ray_angle = _playerA + _fov / 2 - x * _fov / _screen_width;
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

                    if (test_cell == '#')
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
                        _map[testY * _map_width + testX] = '.';
                }
            }

            int ceiling = (int)(_screen_height / 2d - _screen_height * _fov / distance_to_wall);
            int floor = _screen_height - ceiling;
            char wall_shade;

            if (is_bounds)
                wall_shade = '|';
            else if (distance_to_wall <= _depth / 4d)
                wall_shade = '\u2588';
            else if (distance_to_wall < _depth / 3d)
                wall_shade = '\u2593';
            else if (distance_to_wall < _depth / 2d)
                wall_shade = '\u2592';
            else if (distance_to_wall < _depth)
                wall_shade = '\u2591';
            else
                wall_shade = ' ';

            for (int y = 0; y < _screen_height; y++)
            {
                if (y <= ceiling)
                    result[y * _screen_width + x] = ' ';
                else if (y > ceiling && y <= floor)
                    result[y * _screen_width + x] = wall_shade;
                else
                {
                    char floor_shade;
                    double b = 1 - (y - _screen_height / 2d) / (_screen_height / 2d);

                    if (b < 0.25)
                        floor_shade = '#';
                    else if (b < 0.5)
                        floor_shade = 'x';
                    else if (b < 0.75)
                        floor_shade = '-';
                    else if (b < 0.9)
                        floor_shade = '.';
                    else
                        floor_shade = ' ';

                    result[y * _screen_width + x] = floor_shade;
                }
            }
            return result;
        }

        private static void InitMap()
        {
            string[] map = File.ReadAllLines("MAP.txt");

            _map.Clear();
            foreach (string line in map)
                _map.Append(line);
        }
    }
}