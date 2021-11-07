using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tactics_Script_Tool
{
    static class Binary
    {
        public static int GetAlignedValue(int value, int alignment)
        {
            return value + alignment - 1 & ~(alignment - 1);
        }
    }
}
