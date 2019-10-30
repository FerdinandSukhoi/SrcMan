using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace SrcMan
{
    public class MobileSuit
    {
        public Assembly Assembly { get; set; }
        public Stream Stream { get; set; }
        public MobileSuit()
        {
            Assembly = Assembly.GetExecutingAssembly();
        }

        public int Run(string prompt)
        {

            for (; ; )
            {
                string cmd = Console.ReadLine();
            }

            return 0;
        }

        public int Run()
        {

            for(; ; )
            {
                string cmd = Console.ReadLine();
            }

            return 0;
        }
    }
}
