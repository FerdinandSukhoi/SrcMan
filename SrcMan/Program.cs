using System;
using System.Reflection;

namespace SrcMan
{
    class Program
    {
        static void Main(string[] args)
        {
            var ms = new MobileSuit(typeof(MobileSuitTest));
            ms.Run();
        }

    }
}
