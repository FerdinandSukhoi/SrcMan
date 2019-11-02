using System;
using System.Reflection;

namespace SrcMan
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""==null);

            var ms = new MobileSuit(typeof(SrcMan));
            ms.Run();
        }

    }
}
