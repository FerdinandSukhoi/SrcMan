using System;
using System.Reflection;
using PlasticMetal.MobileSuit;

namespace SrcMan
{
    class Program
    {
        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var ms = new MsHost(typeof(SrcMan));
            ms.RunScripts(new[] {"load"});
            ms.Run();
        }

    }
}
