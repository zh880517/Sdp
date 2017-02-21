using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using ITDM;
using CodeGen;
using System.IO;

namespace Parser
{
    public static class Builder
    {

        public static void Gen(ConsoleCmdLine cmd)
        {
            List<ICodeGen> list = new List<ICodeGen>();
            if (cmd["csharp"].Exists)
            {
                list.AddRange(CreadCSharpGen(cmd["src"].Value, cmd["csharp"].Value));
            }
            List<Task> tasks = new List<Task>();
            foreach (var gen in list)
            {
                Task task = Task.Factory.StartNew(()=> GenCode(gen));
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static List<ICodeGen> CreadCSharpGen(string srcDir, string outDir)
        {
            srcDir = FileHelper.TranPath(srcDir);
            outDir = FileHelper.TranPath(outDir);
            List<ICodeGen> list = new List<ICodeGen>();
            foreach (var file in ProtoPackage.Files)
            {
                GenCSharp gen = new GenCSharp();
                gen.Init(srcDir, outDir, file.Value);
                list.Add(gen);
            }
            return list;
        }

        private static void GenCode(ICodeGen gen)
        {
            string strOut = gen.ToCode();
            string strOutFile = gen.GetOutFileName();
            if (File.Exists(strOutFile))
            {
                StreamReader sr = new StreamReader(strOutFile);
                string file = sr.ReadToEnd();
                sr.Close();
                if (file == strOut)
                    return;
            }
            string dir = Path.GetDirectoryName(strOutFile);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream fs = new FileStream(strOutFile, FileMode.Create))
            {
                byte[] data = new UTF8Encoding().GetBytes(strOut);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
            }
        }

    }
}
