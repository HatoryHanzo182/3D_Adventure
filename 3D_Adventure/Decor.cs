using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
     // Decor enumeration type, contains all
    // char elements drawn in the console.
    public enum Decor
    {
        wall = '#',
        wall_4d = '\u2588',
        wall_3d = '\u2593',
        wall_2d = '\u2592',
        wall_d = '\u2591',
        space = ' ',
        floor = '.',
        bound = '|'
    }
}
