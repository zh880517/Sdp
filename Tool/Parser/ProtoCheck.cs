using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public static class ProtoCheck
    {

        public static bool CheckType(Token token, string nameSpace, ref Token result)
        {
            if (ProtoParser.BaseType.Contains(token.Value))
                return true;
            var param = token.Value.Split('.');
            if (param.Length == 2)
            {
                result = FindName(param[0], param[1]);
                return null != result;
            }
            else if (param.Length == 1)
            {
                result = FindName(nameSpace, token.Value);
                return null != result;
            }
            return false;
        }

        public static bool Check(this EnumEntity entity, string nameSpace)
        {
            if (entity.Name == null)
                return false;
            for (int i=0; i<entity.Fields.Count; ++i)
            {
                var field = entity.Fields[i];
                if (field.Name.Value.IndexOf("MSG_ID_") == 0 
                    || field.Name.Value.IndexOf("RPC_") == 0)
                {
                    if (field.Value == null)
                    {
                        Console.WriteLine(string.Format("{0} => Msg or RPC ID must have value, like: MSG_ID_XXX = 12345, or RPC_service_func=123", field.Name));
                        return false;
                    }
                    MsgIDEntity idEntity = new MsgIDEntity()
                    {
                        FieldToken = field.Name,
                        ValueToken = field.Value,
                        EnumToken = entity.Name,
                    };
                    if (!AddMsgIdEntity(nameSpace, idEntity))
                        return false;
                }
                for (int j=i+1; j<entity.Fields.Count; ++j)
                {
                    if (field.Name.Value == entity.Fields[j].Name.Value)
                    {
                        Console.WriteLine(string.Format("{0} => Enum field name repeated", field.Name));
                        return false;
                    }
                }
            }
            var token = FindName(nameSpace, entity.Name);
            if (token != null)
            {
                Console.WriteLine(string.Format("{0} => Enum name has exist in {1}", entity.Name, token));
                return false;
            }
            return true;
        }

        public static bool Check(this TypeEntity entity, string nameSpace)
        {
            entity.TypeType = FieldType.BaseType;
            if (entity.Type.Value == "void")
            {
                entity.TypeType = FieldType.Void;
            }
            else if (!ProtoParser.BaseType.Contains(entity.Type.Value))
            {
                if (ProtoParser.VectorType.Contains(entity.Type.Value))
                {
                    entity.TypeType = FieldType.Vector;
                    Token result = null;
                    if (!CheckType(entity.TypeParam[0], nameSpace, ref result))
                    {
                        Console.WriteLine(string.Format("{0} => Type {1} not exist!", entity.TypeParam[0], entity.TypeParam[0].Value));
                        return false;
                    }
                    if (result != null)
                        AddIncude(entity.Type.FileName, result.FileName);
                    entity.ParamType.Add(ToFieldType(result));
                }
                else if (ProtoParser.MapType.Contains(entity.Type.Value))
                {
                    entity.TypeType = FieldType.Map;
                    if (!ProtoParser.BaseType.Contains(entity.TypeParam[0].Value))
                    {
                        Console.WriteLine(string.Format("{0} => Map Key must be base type!"), entity.TypeParam[0]);
                        return false;
                    }
                    entity.ParamType.Add(FieldType.BaseType);
                    Token result = null;
                    if (!CheckType(entity.TypeParam[1], nameSpace, ref result))
                    {
                        Console.WriteLine(string.Format("{0} => Type {1} not exist!", entity.TypeParam[0], entity.TypeParam[0].Value));
                        return false;
                    }
                    if (result != null)
                        AddIncude(entity.Type.FileName, result.FileName);
                    entity.ParamType.Add(ToFieldType(result));
                }
                else
                {
                    Token result = null;
                    if (!CheckType(entity.Type, nameSpace, ref result))
                    {
                        Console.WriteLine(string.Format("{0} => Type {1} not exist!", entity.Type, entity.Type.Value));
                        return false;
                    }
                    if (result != null)
                        AddIncude(entity.Type.FileName, result.FileName);

                    entity.TypeType = ToFieldType(result);
                }
            }
            return true;
        }

        public static bool Check(this StructField field, string nameSpace)
        {
            if (field.Index.IntValue < 0)
            {
                Console.WriteLine(string.Format("{0} => Struct index must be equal or  greater than 0", field.Index));
                return false;
            }
            return field.Type.Check(nameSpace);
        }

        public static bool Check(this StructEntity entity, string nameSpace)
        {
            if (entity.Name == null)
                return false;
            var token = FindName(nameSpace, entity.Name);
            if (token != null)
            {
                Console.WriteLine(string.Format("{0} => Struct name has exist in {1}", entity.Name, token));
                return false;
            }
            HashSet<int> vIndex = new HashSet<int>();
            for (int i = 0; i < entity.Fields.Count; ++i)
            {
                var field = entity.Fields[i];
                if (!field.Check(nameSpace))
                    return false;
                if (vIndex.Contains(field.Index.IntValue))
                {
                    Console.WriteLine(string.Format("{0} => Struct index repeated", field.Index));
                    return false;
                }
                vIndex.Add(field.Index.IntValue);
                for (int j = i + 1; j < entity.Fields.Count; ++j)
                {
                    if (field.Name.Value == entity.Fields[j].Name.Value)
                    {
                        Console.WriteLine(string.Format("{0} => Struct field name repeated", field.Name));
                        return false;
                    }
                }
            }
            if (entity.IsMessage)
            {
                string idName = string.Format("MSG_ID_{0}", entity.Name.Value);
                var idEntity = FindMsgIdEntity(nameSpace, idName);
                if (idEntity == null)
                {
                    Console.WriteLine(string.Format("{0} => message need a ID enum in same namespace, like : MSG_ID_messageName", entity.Name));
                    return false;
                }
                var msgEntity = (MessageEnity)entity;
                msgEntity.ID = idEntity;
            }
            return true;
        }

        public static bool Check(this RpcEntity entity, string nameSpace, string serviceName)
        {
            if (!entity.ReturnType.Check(nameSpace))
                return false;
            if (entity.Param != null)
            {
                if (!entity.Param.Type.Check(nameSpace))
                    return false;
            }
            string idName = string.Format("RPC_{0}_{1}", serviceName, entity.Name.Value);
            var idEntity = FindMsgIdEntity(nameSpace, idName);
            if (idEntity == null)
            {
                Console.WriteLine(string.Format("{0} => rpc need a ID enum in same namespace, like : RPC_service_rpcName", entity.Name));
                return false;
            }
            entity.ID = idEntity;
            return true;
        }

        public static bool Check(this ServiceEntity entity, string nameSpace)
        {
            if (entity.Name == null)
                return false;
            var token = FindName(nameSpace, entity.Name);
            if (token != null)
            {
                Console.WriteLine(string.Format("{0} => service name has exist in {1}", entity.Name, token));
                return false;
            }
            for (int i=0; i< entity.Rpcs.Count; ++i)
            {
                var rpc = entity.Rpcs[i];
                if (!rpc.Check(nameSpace, entity.Name.Value))
                    return false;
                for (int j=i+1; j<entity.Rpcs.Count; ++j)
                {
                    if (rpc.Name.Value == entity.Rpcs[j].Name.Value)
                    {
                        Console.WriteLine(string.Format("{0} => rpc name has exist in {1}", entity.Rpcs[j].Name, rpc.Name));
                        return false;
                    }
                }
            }
            return true;
        }

//         public static bool Check(this NameSpaceEntity entity)
//         {
//             foreach(var en in entity.Enums)
//             {
//                 if (!en.Check(entity.Name.Value))
//                     return false;
//             }
//             foreach (var st in entity.Structs)
//             {
//                 if (!st.Check(entity.Name.Value))
//                     return false;
//             }
//             foreach (var sv in entity.Services)
//             {
//                 if (!sv.Check(entity.Name.Value))
//                     return false;
//             }
//             return true;
//         }

        public static bool CheckEnum(this FileEntity entity)
        {
            foreach(var ns in entity.NameSpaces)
            {
                foreach (var en in ns.Enums)
                {
                    if (!en.Check(ns.Name.Value))
                        return false;
                }
            }
            return true;
        }

        public static bool CheckStruct(this FileEntity entity)
        {
            foreach (var ns in entity.NameSpaces)
            {
                foreach (var st in ns.Structs)
                {
                    if (!st.Check(ns.Name.Value))
                        return false;
                }
            }
            return true;
        }

        public static bool CheckService(this FileEntity entity)
        {
            foreach (var ns in entity.NameSpaces)
            {
                foreach (var se in ns.Services)
                {
                    if (!se.Check(ns.Name.Value))
                        return false;
                }
            }
            return true;
        }
        
        public static Token HaveName(this FileEntity entity, string nameSpace, Token name)
        {
            foreach(var ns in entity.NameSpaces)
            {
                if (ns.Name.Value == nameSpace)
                {
                    foreach(var st in ns.Structs)
                    {
                        if (st.Name != name && st.Name.Value == name.Value)
                            return st.Name;
                    }
                    foreach (var st in ns.Enums)
                    {
                        if (st.Name != name && st.Name.Value == name.Value)
                            return st.Name;
                    }
                }
            }
            return null;
        }

        public static Token HaveName(this FileEntity entity, string nameSpace, string name)
        {
            foreach (var ns in entity.NameSpaces)
            {
                if (ns.Name.Value == nameSpace)
                {
                    foreach (var st in ns.Structs)
                    {
                        if (st.Name.Value == name)
                            return st.Name;
                    }
                    foreach (var st in ns.Enums)
                    {
                        if (st.Name.Value == name)
                            return st.Name;
                    }
                }
            }
            return null;
        }

        public static Token FindName(string nameSpace, Token name)
        {
            foreach(var file in ProtoPackage.Files)
            {
                var token = file.Value.HaveName(nameSpace, name);
                if (token != null)
                    return token;
            }
            return null;
        }
        

        public static Token FindName(string nameSpace, string name)
        {
            foreach (var file in ProtoPackage.Files)
            {
                var token = file.Value.HaveName(nameSpace, name);
                if (token != null)
                    return token;
            }
            return null;
        }

        public static bool AddMsgIdEntity(string nameSpace, MsgIDEntity idEntity)
        {
            if (string.IsNullOrEmpty(nameSpace))
                nameSpace = "0";
            if (!ProtoPackage.MsgIDs.ContainsKey(nameSpace))
                ProtoPackage.MsgIDs.Add(nameSpace, new List<MsgIDEntity>());
            foreach (var id in ProtoPackage.MsgIDs[nameSpace])
            {
                if (id.FieldToken.Value == idEntity.FieldToken.Value )
                {
                    Console.WriteLine(string.Format("{0} ID name repeated in same namespace {1}", idEntity.FieldToken, id.FieldToken));
                    return false;
                }
                if (id.ValueToken.IntValue == idEntity.ValueToken.IntValue)
                {
                    Console.WriteLine(string.Format("{0} ID value repeated in same namespace {1}", idEntity.FieldToken, id.FieldToken));
                    return false;
                }
            }
            ProtoPackage.MsgIDs[nameSpace].Add(idEntity);
            return true;
        }

        public static MsgIDEntity FindMsgIdEntity(string nameSpace, string name)
        {
            if (string.IsNullOrEmpty(nameSpace))
                nameSpace = "0";
            if (ProtoPackage.MsgIDs.ContainsKey(nameSpace))
            {
                var list = ProtoPackage.MsgIDs[nameSpace];
                return list.FirstOrDefault(obj => obj.FieldToken.Value == name);
            }
            return null;
        }

        public static void AddIncude(string file, string include)
        {
            if (file != include && ProtoPackage.Files.ContainsKey(file))
            {
                var fileEntity = ProtoPackage.Files[file];
                if (!fileEntity.Includes.Contains(include))
                    fileEntity.Includes.Add(include);
            }
        }

        public static FieldType ToFieldType(Token token)
        {
            if (token == null || ProtoParser.BaseType.Contains(token.Value))
                return FieldType.BaseType;
            if (ProtoParser.VectorType.Contains(token.Value))
                return FieldType.Vector;
            if (ProtoParser.MapType.Contains(token.Value))
                return FieldType.Map;
            if (ProtoPackage.EnumNames.Contains(token))
                return FieldType.Enum;
            if (token.Value == "void")
                return FieldType.Void;
            return FieldType.Struct;
        }
    }
}
