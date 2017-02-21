using System;
using ITDM;
using System.IO;

namespace Parser
{

    class Program
    {
        static void Main(string[] args)
        {
            /*
             * cmd example : -src "E:/proto" -csharp "E:/GenCSharp"
             */


            try
            {
                ConsoleCmdLine c = new ConsoleCmdLine();
                CmdLineString srcDir = new CmdLineString("src", true, "源文件目录");
                CmdLineString csharpDir = new CmdLineString("csharp", false, "生成C#文件的目录");
                CmdLineString cppDir = new CmdLineString("cpp", false, "生成C++文件的目录");
                c.RegisterParameter(srcDir);
                c.RegisterParameter(csharpDir);
                c.RegisterParameter(cppDir);
                c.Parse(args);
                bool bResult = ProtoPackage.Parse(srcDir.Value);
                if (bResult)
                {
                    Builder.Gen(c);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Please press 'Enter' key to continue ... ");
            Console.ReadLine();
        }
    }
}
