using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser;
namespace CodeGen
{
    public interface ICodeGen
    {
        void Init(string srcDir, string outDir, FileEntity entity);
        string GetOutFileName();
        string ToCode();

    }

    public static class CodeGenHelper
    {
        public static StringBuilder AppendTable(this StringBuilder sb, int tableNum)
        {
            for (int i=0; i< tableNum; ++i)
                sb.Append('\t');
            return sb;
        }

        public static StringBuilder NewLine(this StringBuilder sb, int tableNum = 0)
        {
            sb.Append('\n');
            for (int i = 0; i < tableNum; ++i)
                sb.Append('\t');
            return sb;
        }
    }
}
