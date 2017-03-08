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
        Void,
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


    public class TypeEntity
    {
        public Token Type;
        public List<Token> TypeParam = new List<Token>();
        public FieldType TypeType;
        public List<FieldType> ParamType = new List<FieldType>();
    }

    public class StructField
    {
        public Token Name;
        public Token Index;
        public TypeEntity Type;
    }

    public class StructEntity
    {
        public Token Name;
        public List<StructField> Fields = new List<StructField>();
        public bool IsMessage = false;
    }
    
    public class MsgIDEntity
    {
        public Token FieldToken;
        public Token ValueToken;
        public Token EnumToken;
    }

    public class MessageEnity : StructEntity
    {
        public MessageEnity()
        {
            IsMessage = true;
        }
        public MsgIDEntity ID;
    }


    public class ParamEntity
    {
        public Token Name;
        public TypeEntity Type;
    }
    
    public class RpcEntity
    {
        public Token Name;
        public TypeEntity ReturnType;
        public ParamEntity Param;
        public MsgIDEntity ID;
    }

    public class ServiceEntity
    {
        public Token Name;
        public List<RpcEntity> Rpcs = new List<RpcEntity>();
    }

    public class NameSpaceEntity
    {
        public Token Name;
        public List<EnumEntity> Enums = new List<EnumEntity>();
        public List<StructEntity> Structs = new List<StructEntity>();
        public List<ServiceEntity> Services = new List<ServiceEntity>();
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

        public static Dictionary<string, List<MsgIDEntity>> MsgIDs = new Dictionary<string, List<MsgIDEntity>>();

        public static bool Parse(string strDir)
        {
            if (!ProtoParser.ParseProtoPackage(strDir))
                return false;

            foreach (var fe in Files)
            {
                if (!fe.Value.CheckEnum())
                    return false;
            }
            foreach (var fe in Files)
            {
                if (!fe.Value.CheckStruct())
                    return false;
            }
            foreach (var fe in Files)
            {
                if (!fe.Value.CheckService())
                    return false;
            }
            return true;
        }
    }
}
