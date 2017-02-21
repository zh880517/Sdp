using System;
using System.Collections.Generic;

namespace Parser
{
    public enum FieldType
    {
        BaseType,
        Vector,
        Map,
        Struct,
        Enum,
    }
    
    public class EnumField
    {
        public Token Name;
        public Token Value;
    }

    public class EnumEntity
    {
        public Token Name;
        public List<EnumField> Fields = new List<EnumField>(); 
    }

    public class StructField
    {
        public Token Name;
        public Token Type;
        public List<Token> TypeParam = new List<Token>();
        public Token Index;
        public FieldType TypeType;
        public List<FieldType> ParamType = new List<FieldType>();
    }

    public class StructEntity
    {
        public Token Name;
        public List<StructField> Fields = new List<StructField>();
    }

    public class NameSpaceEntity
    {
        public Token Name;
        public List<EnumEntity> Enums = new List<EnumEntity>();
        public List<StructEntity> Structs = new List<StructEntity>();
    }

    public class FileEntity
    {
        public string FileName;
        public List<string> Includes = new List<string>();
        public List<NameSpaceEntity> NameSpaces = new List<NameSpaceEntity>();
    }
    
    public static class ProtoPackage
    {
        public static Dictionary<string, FileEntity> Files = new Dictionary<string, FileEntity>();

        public static List<Token> EnumNames = new List<Token>();
        public static List<Token> StructNames = new List<Token>();

        public static bool Parse(string strDir)
        {
            if (!ProtoParser.ParseProtoPackage(strDir))
                return false;

            foreach (var fe in Files)
            {
                if (!fe.Value.Check())
                    return false;
            }

            return true;
        }
    }
}
