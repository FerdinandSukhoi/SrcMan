using System;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [MobileSuitItem, MobileSuitInfo("Test")]
    public class MobileSuitTest : IMobileSuitInfo
    {
        public string Prompt { get; set; } = "Test";
        public void Test()
        {
            Console.WriteLine("Test!!!!");
        }
    }
}
