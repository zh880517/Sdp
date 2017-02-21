using System;
using ITDM;
using System.IO;

namespace Parser
{

    class Program
    {
        static void Main(string[] args)
        {
            bool bResult = Builder.DoBuilder(args);

            Console.WriteLine("Please press 'Enter' key to continue ... ");
            Console.ReadLine();
        }
    }
}
