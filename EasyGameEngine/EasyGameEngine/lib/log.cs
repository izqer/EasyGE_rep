using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGameEngine
{
    class log
    {
        // Режим разработки (для консоли)
        private static bool debug = true;

        public static void Log(string text)
        {
            if (debug)
                Console.WriteLine(text);
        }
    }
}
