using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public enum ETokenType
    {
        eSpace,
        eSeparate,
        eToken,
        eString,
        eComments
    }

    public class Token
    {
        public string Value;

        public ETokenType Type = ETokenType.eSpace;

        public int LineNum = 0;

        public string FileName;

        public bool Empty
        {
            get{ return Type == ETokenType.eSpace; }
        }

        public int IntValue { get { return int.Parse(Value); } }


        override public string ToString()
        {
            return string.Format("File: {0}, Line: {1}, Near: {2}", FileName, LineNum, Value);
        }
    }
}
