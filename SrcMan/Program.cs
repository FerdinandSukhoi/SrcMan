using System;
using System.Reflection;

namespace SrcMan
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var m = assembly.GetType("SrcMan.Test").GetMethod("testM");
            m.Invoke();
            Console.WriteLine("Hello World!");
        }

    }
}
