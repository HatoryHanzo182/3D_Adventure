using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
      // Control contains four constants.
     // Each constant is associated with a corresponding key on the keyboard,
    // represented using enum ConsoleKey.
    public enum Control
    {
        forward = ConsoleKey.W,
        back = ConsoleKey.S,
        left = ConsoleKey.A,
        right = ConsoleKey.D
    }
}
