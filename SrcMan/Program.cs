using System;
using System.Reflection;
using MobileSuit;

namespace SrcMan
{
    class Program
    {
        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var ms = new MobileSuitHost(typeof(SrcMan));
            ms.Run();
        }

    }
}
