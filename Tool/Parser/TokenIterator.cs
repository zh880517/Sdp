using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class TokenIterator
    {
        private List<Token> m_list;

        private int m_iIndex;

        public TokenIterator(List<Token> list, int iIndex = 0)
        {
            m_list = list;
            m_iIndex = iIndex;
        }

        public static TokenIterator operator ++(TokenIterator it)
        {
            it.m_iIndex++;
            return it;
        }

        public static TokenIterator operator --(TokenIterator it)
        {
            it.m_iIndex--;
            return it;
        }
        

        public Token Value
        {
            get
            {
                if (m_iIndex < 0 || m_iIndex >= m_list.Count)
                    return null;
                return m_list[m_iIndex];
            }
        }

        public int CurIndex
        {
            get { return m_iIndex; }
            set { m_iIndex = value; }
        }
    }
}
