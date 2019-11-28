using System;
using System.Reflection;

namespace SrcMan
{
    class Program
    {
        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var ms = new MobileSuit(typeof(SrcMan));
            ms.Run();
        }

    }
}
