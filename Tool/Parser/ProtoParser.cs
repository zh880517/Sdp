using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Parser
{
    public static class ProtoParser
    {
        public static List<string> BaseType = new List<string>(){
            "int", "uint" , "long", "ulong", "bool",
            "float", "double" , "string", "bytes", "Guid", "DateTime"
        };

        public static List<string> VectorType = new List<string>() { "Vector", "List" };

        public static List<string> MapType = new List<string>() { "Map", "Directory" };

        private static RegexMatch IntMatch = new RegexMatch("^[+-]?([0-9]{1,})$");
        private static RegexMatch UIntMatch = new RegexMatch("^[+]?([0-9]{1,})$");
        private static RegexMatch VarNameMatch = new RegexMatch("^[a-zA-Z_]([a-zA-Z0-9_]{1,})$");
        private static RegexMatch TypeNameMatch = new RegexMatch("^[a-zA-Z_]([a-zA-Z0-9_]{1,})([.][a-zA-Z_]([a-zA-Z0-9_]{1,}))?$");
        private static SymbolMatch CommaMatch = new SymbolMatch(',');
        private static SymbolMatch SemicolonMatch = new SymbolMatch(';');
        private static SymbolMatch EquallSignMatch = new SymbolMatch('=');
        private static SymbolMatch OpeningbraceMatch = new SymbolMatch('{');
        private static SymbolMatch CloseingbraceMatch = new SymbolMatch('}');
        private static SymbolMatch OpeningAngleMatch = new SymbolMatch('<');
        private static SymbolMatch CloseingAngleMatch = new SymbolMatch('>');
        private static KeyWordMatch EnumMatch = new KeyWordMatch("enum");
        private static KeyWordMatch StructMatch = new KeyWordMatch("struct");
        private static KeyWordMatch ImportMatch = new KeyWordMatch("import");
        private static KeyWordMatch NamespaceMatch = new KeyWordMatch("namespace");
        private static StringMatch NormalStringMatch = new StringMatch();
        private static KeyWordsMatch BaseTypeMatch = new KeyWordsMatch(BaseType);
        private static KeyWordsMatch VectorMatch = new KeyWordsMatch(VectorType);
        private static KeyWordsMatch MapMatch = new KeyWordsMatch(MapType);
        
        public static string ItToError(TokenIterator it)
        {
           return "Parse faile in file : " + it.Value.FileName + " in line : " + it.Value.LineNum + " near : " + it.Value.Value;
        }

        public static void ThrowError(TokenIterator it)
        {
            throw new Exception(ItToError(it));
        }

        public static EnumField ParseEnumField(TokenIterator it)
        {
            EnumField field = new EnumField();
            do
            {
                if (VarNameMatch.Match(it, ref field.Name))
                {
                    if (EquallSignMatch.Match(it))
                        if (IntMatch.Match(it, ref field.Value) && CommaMatch.Match(it))
                            break;
                    if (CommaMatch.Match(it))
                        break;
                    if (CloseingbraceMatch.Match(it))
                    {
                        --it;
                        break;
                    }
                }
                ThrowError(it);
            } while (false);
            return field;
        }

        public static EnumEntity ParseEnumEntity(TokenIterator it)
        {
            if (EnumMatch.Match(it))
            {
                EnumEntity entity = new EnumEntity();
                if (VarNameMatch.Match(it, ref entity.Name) && OpeningbraceMatch.Match(it))
                {
                    while (true)
                    {
                        if (CloseingbraceMatch.Match(it))
                            break;
                        var field = ParseEnumField(it);
                        entity.Fields.Add(field);
                    }
                    return entity;
                }
                throw new Exception(ItToError(it));
            }
            return null;
        }

        public static StructField ParseStructField(TokenIterator it)
        {
            StructField field = new StructField();
            if (IntMatch.Match(it, ref field.Index))
            {
                do
                {
                    if (VectorMatch.Match(it, ref field.Type))
                    {
                        Token token = null;
                        if (!OpeningAngleMatch.Match(it) || !TypeNameMatch.Match(it, ref token) || !CloseingAngleMatch.Match(it))
                            break;
                        field.TypeParam.Add(token);
                    }
                    else if (MapMatch.Match(it, ref field.Type))
                    {
                        Token token1 = null;
                        Token token2 = null;
                        if (!OpeningAngleMatch.Match(it) || !BaseTypeMatch.Match(it, ref token1) || !CommaMatch.Match(it) || !TypeNameMatch.Match(it, ref token2) || !CloseingAngleMatch.Match(it))
                            break;
                        field.TypeParam.Add(token1);
                        field.TypeParam.Add(token2);
                    }
                    else if (!BaseTypeMatch.Match(it, ref field.Type) && !TypeNameMatch.Match(it, ref field.Type))
                    {
                        break;
                    }
                    if (!VarNameMatch.Match(it, ref field.Name))
                        break;
                    if (!SemicolonMatch.Match(it))
                        break;

                    return field;
                } while (false);
                ThrowError(it);
            }
            return null;
        }

        public static StructEntity ParseStructEntity(TokenIterator it)
        {
            if (StructMatch.Match(it))
            {
                StructEntity entity = new StructEntity();
                if (VarNameMatch.Match(it, ref entity.Name))
                {
                    if (OpeningbraceMatch.Match(it))
                    {
                        while (true)
                        {
                            if (CloseingbraceMatch.Match(it))
                                break;
                            var filed = ParseStructField(it);
                            if (filed == null)
                                ThrowError(it);
                            entity.Fields.Add(filed);
                        }
                        return entity;
                    }
                    ThrowError(it);
                }
            }
            return null;
        }

        public static NameSpaceEntity ParseNameSpace(TokenIterator it)
        {
            if (NamespaceMatch.Match(it))
            {
                NameSpaceEntity entity = new NameSpaceEntity();
                if (VarNameMatch.Match(it, ref entity.Name) && OpeningbraceMatch.Match(it))
                {
                    while (true)
                    {
                        if (CloseingbraceMatch.Match(it))
                            break;
                        var enumEntity = ParseEnumEntity(it);
                        if (enumEntity != null)
                            entity.Enums.Add(enumEntity);
                        var structEntity = ParseStructEntity(it);
                        if (structEntity != null)
                            entity.Structs.Add(structEntity);
                    }
                    return entity;
                }
                ThrowError(it);
            }
            return null;
        }

        /*
        public static List<Token> ParseImport(TokenIterator it)
        {
            List<Token> list = new List<Token>();
            while (true)
            {
                if (!ImportMatch.Match(it))
                    break;
                Token token = null;
                if (NormalStringMatch.Match(it, ref token) && SemicolonMatch.Match(it))
                {
                    list.Add(token);
                    continue;
                }
                ThrowError(it);
            }
            return list;
        }
        */
        public static FileEntity ParseFile(string strFileName)
        {
            Tokenizer tokenizer = new Tokenizer();
            if (tokenizer.Load(strFileName))
            {
                TokenIterator it = tokenizer.Iterator();
                FileEntity fileEnity = new FileEntity();
                fileEnity.FileName = strFileName;
                //fileEnity.Imports = ParseImport(it);
                NameSpaceEntity globalNS = new NameSpaceEntity() { Name = new Token()};
                while(true)
                {
                    var entity = ParseNameSpace(it);
                    if (entity == null)
                        break;
                    fileEnity.NameSpaces.Add(entity);
                    var enumEntity = ParseEnumEntity(it);
                    if (enumEntity != null)
                        globalNS.Enums.Add(enumEntity);
                    var stEntity = ParseStructEntity(it);
                    if (stEntity != null)
                        globalNS.Structs.Add(stEntity);
                    
                }
                if (globalNS.Enums.Count > 0 || globalNS.Structs.Count > 0)
                    fileEnity.NameSpaces.Add(globalNS);

                return fileEnity;
            }
            return null;
        }

        public static bool ParseProtoPackage(string strDir)
        {
            var Files = FileHelper.GetAllFile(strDir, "*.sdp");

            ConcurrentDictionary<string, FileEntity> fileEntitys = new ConcurrentDictionary<string, FileEntity>();
            List<Task> tasks = new List<Task>();
            foreach (var file in Files)
            {
                var fileName = file.Replace('\\', '/');
                Task task = Task.Factory.StartNew(() =>
                {
                    var entity = ParseFile(fileName);
                    if (entity == null)
                    {
                        throw new Exception("Parse file in file :" + file);
                    }
                    fileEntitys.TryAdd(entity.FileName, entity);
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var kv in fileEntitys.ToArray())
            {
                ProtoPackage.Files.Add(kv.Key, kv.Value);
            }

            foreach(var file in ProtoPackage.Files)
            {
                foreach (var ns in file.Value.NameSpaces)
                {
                    foreach (var en in ns.Enums)
                    {
                        ProtoPackage.EnumNames.Add(en.Name);
                    }
                    foreach (var st in ns.Structs)
                    {
                        ProtoPackage.StructNames.Add(st.Name);
                    }
                }
            }
            return true;
        }
        
    }
}
