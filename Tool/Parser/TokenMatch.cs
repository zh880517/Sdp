using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Parser
{
    public interface ITokenMatch
    {
        bool Match(TokenIterator it, ref Token token);
        bool Match(TokenIterator it);
    }

    public class StringMatch : ITokenMatch
    {
        public bool Match(TokenIterator it)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eString)
            {
                ++it;
                return true;
            }
            return false;
        }

        public bool Match(TokenIterator it, ref Token token)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eString)
            {
                token = it.Value;
                ++it;
                return true;
            }
            return false;
        }
    }

    public class KeyWordMatch : ITokenMatch
    {
        private string str;

        public KeyWordMatch(string val)
        {
            str = val;
        }

        public bool Match(TokenIterator it)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eToken && it.Value.Value == str)
            {
                ++it;
                return true;
            }
            return false;
        }

        public bool Match(TokenIterator it, ref Token token)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eToken && it.Value.Value == str)
            {
                token = it.Value;
                ++it;
                return true;
            }
            return false;
        }
    }

    public class KeyWordsMatch : ITokenMatch
    {
        private List<string> strs;

        public KeyWordsMatch(List<string> val)
        {
            strs = val;
        }

        public bool Match(TokenIterator it)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eToken && strs.Contains(it.Value.Value))
            {
                ++it;
                return true;
            }
            return false;
        }

        public bool Match(TokenIterator it, ref Token token)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eToken && strs.Contains(it.Value.Value))
            {
                token = it.Value;
                ++it;
                return true;
            }
            return false;
        }
    }

    public class SymbolMatch : ITokenMatch
    {
        private string sep;

        public SymbolMatch(char val)
        {
            sep = new string(val, 1);
        }

        public bool Match(TokenIterator it)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eSeparate && it.Value.Value == sep)
            {
                ++it;
                return true;
            }
            return false;
        }

        public bool Match(TokenIterator it, ref Token token)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eSeparate && it.Value.Value == sep)
            {
                token = it.Value;
                ++it;
                return true;
            }
            return false;
        }
        
    }

    public class RegexMatch :ITokenMatch
    {
        private string regex;

        public RegexMatch(string val)
        {
            regex = val;
        }

        public bool Match(TokenIterator it)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eToken && Regex.IsMatch(it.Value.Value, regex))
            {
                ++it;
                return true;
            }
            return false;
        }

        public bool Match(TokenIterator it, ref Token token)
        {
            if (it.Value != null && it.Value.Type == ETokenType.eToken && Regex.IsMatch(it.Value.Value, regex))
            {
                token = it.Value;
                ++it;
                return true;
            }
            return false;
        }
        
    }
}
