using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Parser
{
    public class Tokenizer
    {
        private string m_strBuffer;

        private string m_strFile;

        private char[] m_vSep = {',', '=', ';', '{', '}', '<', '>', '[', ']' };

        private List<Token> m_listToken;

        private int m_CurrLine;

        private int m_iBegin;

        public bool SkipComments = true;

        public bool IsEnd
        {
            get { return m_strBuffer.Count() == m_iBegin; }
        }

        public bool Load(string strFile)
        {
            m_CurrLine = 1;
            m_iBegin = 0;
            m_listToken = new List<Token>();
            m_strFile = strFile;
            StreamReader sr = new StreamReader(strFile);
            m_strBuffer = sr.ReadToEnd();
            sr.Close();
            if (m_strBuffer == null)
                return false;

            while (true)
            {
                Token stToken = new Token();
                stToken.FileName = m_strFile;
                if (!Peak(stToken))
                    break;
                if (stToken.Type != ETokenType.eComments || !SkipComments)
                    m_listToken.Add(stToken);
            }

            if (!IsEnd)
            {
                Token last = m_listToken.LastOrDefault();
                Console.Write("Tokenizer Error! filename = " + m_strFile + " Line = " + m_CurrLine.ToString());
                if (last.Type != ETokenType.eSeparate)
                {
                    Console.WriteLine(" near " + last.Value);
                }
                return false;
            }
            return true;
        }

        public void SetSeparate(char[] vSeq)
        {
            m_vSep = vSeq;
        }

        public TokenIterator Iterator()
        {
            return new TokenIterator(m_listToken);
        }

        private bool Peak(Token stToken)
        {
            int iStarFlag = m_iBegin;
            int iCount = m_strBuffer.Count();
            for (int i = m_iBegin; i < iCount; i++)
            {
                char c = m_strBuffer[i];
                m_iBegin = i + 1;
                if (CheckNewLine(c))
                {
                    m_CurrLine++;
                }
                if (stToken.Empty)
                {
                    if (!CheckSpece(c))
                    {
                        if (c == '/')
                        {
                            #region 解析注释
                            bool flag4 = i < iCount - 1 && m_strBuffer[i + 1] == '/';
                            if (i >= iCount - 1 || m_strBuffer[i + 1] != '/')
                                return false;
                            
                            stToken.Type = ETokenType.eComments;
                            stToken.LineNum = m_CurrLine;
                            iStarFlag = i;
                            #endregion
                        }
                        else
                        {
                            if (c == '\"')
                            {
                                #region 解析字符串
                                stToken.Type = ETokenType.eString;
                                stToken.LineNum = m_CurrLine;
                                iStarFlag = i;
                                #endregion
                            }
                            else
                            {
                                if (CheckSep(c))
                                {
                                    stToken.Type = ETokenType.eSeparate;
                                    stToken.LineNum = m_CurrLine;
                                    stToken.Value = new string(c, 1);
                                    return true;
                                }
                                stToken.Type = ETokenType.eToken;
                                stToken.LineNum = m_CurrLine;
                                iStarFlag = i;
                            }
                        }
                    }
                }
                else
                {
                    if (stToken.Type == ETokenType.eComments)
                    {
                        if (CheckNewLine(c))
                        {
                            stToken.Value = m_strBuffer.Substring(iStarFlag, i - iStarFlag);
                            return true;
                        }
                    }
                    else
                    {
                        if (stToken.Type == ETokenType.eString)
                        {
                            if (c == '\"')
                            {
                                if (i < 0 ||  m_strBuffer[i - 1] == '\\')
                                {
                                    stToken.Value = m_strBuffer.Substring(iStarFlag, i - iStarFlag + 1);
                                    return true;
                                }
                            }
                            else
                            {
                                if (CheckNewLine(c))
                                    return false;
                            }
                        }
                        else
                        {
                            if (stToken.Type == ETokenType.eToken)
                            {
                                if (CheckSpece(c))
                                {
                                    stToken.Value = m_strBuffer.Substring(iStarFlag, i - iStarFlag);
                                    return true;
                                }
                                if (CheckSep(c) || c == '/' || c == '\"')
                                {
                                    m_iBegin--;
                                    stToken.Value = m_strBuffer.Substring(iStarFlag, i - iStarFlag);
                                    return true;
                                }//if
                            }//if
                        }//else
                    }//else
                }//else
            }//for
            return false;
        }
        

        private bool CheckSpece(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

        private bool CheckNewLine(char c)
        {
            return c == '\n';
        }

        private bool CheckSep(char c)
        {
            if (m_vSep != null)
            {
                char[] vSeq = m_vSep;
                for (int i = 0; i < vSeq.Length; i++)
                {
                    if (c == vSeq[i])
                        return true;
                }
            }
            return false;
        }
    }
}
