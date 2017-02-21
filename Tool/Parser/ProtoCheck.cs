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

        public static bool Check(this StructField field, string nameSpace)
        {
            if (field.Index.IntValue < 0)
            {
                Console.WriteLine(string.Format("{0} => Struct index must be equal or  greater than 0", field.Index));
                return false;
            }
            field.TypeType = FieldType.BaseType;
            if (!ProtoParser.BaseType.Contains(field.Type.Value))
            {
                if (ProtoParser.VectorType.Contains(field.Type.Value))
                {
                    field.TypeType = FieldType.Vector;
                    Token result = null;
                    if (!CheckType(field.TypeParam[0], nameSpace, ref result))
                    {
                        Console.WriteLine(string.Format("{0} => Type {1} not exist!", field.TypeParam[0], field.TypeParam[0].Value));
                        return false;
                    }
                    if (result != null)
                        AddIncude(field.Name.FileName, result.FileName);
                    field.ParamType.Add(ToFieldType(result));
                }
                else if (ProtoParser.MapType.Contains(field.Type.Value))
                {
                    field.TypeType = FieldType.Map;
                    if (!ProtoParser.BaseType.Contains(field.TypeParam[0].Value))
                    {
                        Console.WriteLine(string.Format("{0} => Map Key must be base type!"), field.TypeParam[0]);
                        return false;
                    }
                    field.ParamType.Add(FieldType.BaseType);
                    Token result = null;
                    if (!CheckType(field.TypeParam[1], nameSpace, ref result))
                    {
                        Console.WriteLine(string.Format("{0} => Type {1} not exist!", field.TypeParam[0], field.TypeParam[0].Value));
                        return false;
                    }
                    if (result != null)
                        AddIncude(field.Name.FileName, result.FileName);
                    field.ParamType.Add(ToFieldType(result));
                }
                else
                {
                    Token result = null;
                    if (!CheckType(field.Type, nameSpace, ref result))
                    {
                        Console.WriteLine(string.Format("{0} => Type {1} not exist!", field.Type, field.Type.Value));
                        return false;
                    }
                    if (result != null)
                        AddIncude(field.Name.FileName, result.FileName);

                    field.TypeType = ToFieldType(result);
                }
            }
            return true;
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
            return true;
        }

        public static bool Check(this NameSpaceEntity entity)
        {
            foreach(var en in entity.Enums)
            {
                if (!en.Check(entity.Name.Value))
                    return false;
            }
            foreach (var st in entity.Structs)
            {
                if (!st.Check(entity.Name.Value))
                    return false;
            }
            return true;
        }

        public static bool Check(this FileEntity entity)
        {
            foreach(var ns in entity.NameSpaces)
            {
                if (!ns.Check())
                    return false;
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
            return FieldType.Struct;
        }
    }
}
