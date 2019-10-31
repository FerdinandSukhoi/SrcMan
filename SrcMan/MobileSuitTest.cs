using System;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [MobileSuitItem, MobileSuitInfo("Test")]
    public class MobileSuitTest : IMobileSuitInfo
    {
        public string Prompt { get; set; } = "Test";
        public TestC TestCC { get; set; } = new TestC();
        public TestC tc = new TestC();
        public void Test()
        {
            Console.WriteLine("Test!!!!");
        }
        [MobileSuitItem, MobileSuitInfo("TestC")]
        public class TestC
        {
            public void t()
            {
                Console.WriteLine("t");
            }
        }
    }
}
