using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isMasterMode = true;

            bool a = false;
            bool b = false;
            bool c = false;

            isMasterMode |= a;
            isMasterMode |= b;
            isMasterMode |= c;

            Console.WriteLine(isMasterMode);
            Console.ReadKey();
        }
    }
}
