using System;
using System.Text;
using Parser;

namespace CodeGen
{
    public class GenCSharp : ICodeGen
    {
        private StringBuilder OutSB = new StringBuilder();
        private FileEntity Entity;
        private string OutFileName;


        public void Init(string srcDir, string outDir, FileEntity entity)
        {
            Entity = entity;
            var str = FileHelper.CalRelativePatch(srcDir, Entity.FileName).Replace(".sdp", ".cs");
            OutFileName = outDir + str;
        }
        
        public string GetOutFileName()
        {
            return OutFileName;
        }

        public string ToCode()
        {
            OutSB.Append("using System;").NewLine();
            OutSB.Append("using System.Collections.Generic;").NewLine();
            OutSB.Append("using Sdp;").NewLine();
            OutSB.NewLine();
            foreach (var ns in Entity.NameSpaces)
            {
                OutSB.Append(NameSpaceToString(ns));
            }
            OutSB.NewLine();
            return OutSB.ToString();
        }

        private string EnumToString(EnumEntity entity, int tableNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.NewLine(tableNum);
            sb.AppendFormat("public enum {0}", entity.Name.Value);
            sb.NewLine(tableNum).Append('{');
            foreach(var field in entity.Fields)
            {
                sb.NewLine(tableNum + 1).Append(field.Name.Value);
                if (field.Value != null)
                    sb.Append(" = ").Append(field.Value.Value);
                sb.Append(',');
            }
            sb.NewLine(tableNum).Append('}');
            sb.NewLine();
            return sb.ToString();
        }

        public string BaseTypeTran(string strType)
        {
            if (strType == "bytes")
                return strType;
            return strType;
        }

        public string StructFieldToMemberString(StructField field)
        {
            if (field.TypeType == FieldType.BaseType || field.TypeType == FieldType.Enum)
            {
                return string.Format("public {0} {1};", BaseTypeTran(field.Type.Value), field.Name.Value);
            }
            else if (field.TypeType == FieldType.Vector)
            {
                return string.Format("public List<{0}> {1} = new List<{0}>();"
                    , BaseTypeTran(field.TypeParam[0].Value), field.Name.Value);
            }
            else if (field.TypeType == FieldType.Map)
            {
                return string.Format("public Dictionary<{0}, {1}> {2} = new Dictionary<{0}, {1}>();"
                    , BaseTypeTran(field.TypeParam[0].Value), BaseTypeTran(field.TypeParam[1].Value), field.Name.Value);
            }
            return string.Format("public {0} {1} = new {0}();", field.Type.Value, field.Name.Value);
        }

        public string StructToString(StructEntity entity, int tableNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.NewLine(tableNum).Append("[Serializable]");
            sb.NewLine(tableNum);
            sb.AppendFormat("public class {0} : IStruct", entity.Name.Value);
            sb.NewLine(tableNum).Append('{');
            if (entity.Fields.Count > 0)
            {
                sb.NewLine(tableNum + 1).Append("[NonSerialized]");
                sb.NewLine(tableNum + 1).Append("private static readonly string[] _member_name_ = new string[] { ");
                for (int i = 0; i < entity.Fields.Count; ++i)
                {
                    sb.Append(entity.Fields[i].Name.Value);
                    if (i < entity.Fields.Count - 1)
                        sb.Append(", ");
                }
                sb.Append(" }");
                sb.NewLine();
            }

            foreach (var field in entity.Fields)
            {
                sb.NewLine(tableNum + 1).Append(StructFieldToMemberString(field)).NewLine();
            }
            sb.NewLine();

            sb.NewLine(tableNum + 1).Append("public void Visit(ISdp sdp)");
            sb.NewLine(tableNum + 1).Append('{');
            for (int i=0; i<entity.Fields.Count; ++i)
            {
                var fd = entity.Fields[i];
                sb.NewLine(tableNum + 2).AppendFormat("sdp.Visit({0}, _member_name_[{1}], false, ref {2});", 
                    fd.Index.Value, i, fd.Name.Value);
            }
            sb.NewLine(tableNum + 1).Append("}");
            sb.NewLine(tableNum).Append("}");
            sb.NewLine();
            return sb.ToString();
        }

        private string NameSpaceToString(NameSpaceEntity entity)
        {
            StringBuilder sb = new StringBuilder();
            int tableNum = 0;
            if (!string.IsNullOrEmpty( entity.Name.Value ))
            {
                tableNum = 1;
                sb.AppendFormat("namespace {0}", entity.Name.Value).Append("\n{");
            }

            foreach (var en in entity.Enums)
            {
                sb.Append(EnumToString(en, tableNum));
            }

            foreach (var st in entity.Structs)
            {
                sb.Append(StructToString(st, tableNum));
            }

            if (!string.IsNullOrEmpty(entity.Name.Value))
                sb.Append('}');

            sb.NewLine();
            return sb.ToString();
        }

    }
}
