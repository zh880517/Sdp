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
            OutSB.NewLine();
            foreach (var ns in Entity.NameSpaces)
            {
                OutSB.Append(NameSpaceToString(ns));
            }
            OutSB.NewLine();
            return OutSB.ToString();
        }

        private string MsgIdToString(MsgIDEntity entity)
        {
            return string.Format("(int){0}.{1}", entity.EnumToken.Value, entity.FieldToken.Value);
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

        public string TypeToString(TypeEntity entity)
        {
            if (entity.TypeType == FieldType.BaseType || entity.TypeType == FieldType.Enum)
            {
                return BaseTypeTran(entity.Type.Value);
            }
            else if (entity.TypeType == FieldType.Vector)
            {
                return string.Format("List<{0}>", BaseTypeTran(entity.TypeParam[0].Value));
            }
            else if (entity.TypeType == FieldType.Map)
            {
                return string.Format("Dictionary<{0}, {1}>", BaseTypeTran(entity.TypeParam[0].Value), BaseTypeTran(entity.TypeParam[1].Value));
            }
            return entity.Type.Value;
        }

        public string StructFieldToMemberString(StructField field)
        {
            if (field.Type.TypeType == FieldType.BaseType || field.Type.TypeType == FieldType.Enum)
            {
                return string.Format("public {0} {1};", BaseTypeTran(field.Type.Type.Value), field.Name.Value);
            }
            string typeString = TypeToString(field.Type);
            return string.Format("public {0} {1} = new {0}();", typeString, field.Name.Value);
        }

        public string StructToString(StructEntity entity, int tableNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.NewLine(tableNum).Append("[Serializable]");
            sb.NewLine(tableNum);
            sb.AppendFormat("public class {0} : Sdp.IStruct", entity.Name.Value);
            if (entity.IsMessage)
            {
                sb.Append(", Sdp.IMessage");
            } 
            sb.NewLine(tableNum).Append('{');
            if (entity.Fields.Count > 0)
            {
                sb.NewLine(tableNum + 1).Append("[NonSerialized]");
                sb.NewLine(tableNum + 1).Append("private static readonly string[] _member_name_ = new string[] { ");
                for (int i = 0; i < entity.Fields.Count; ++i)
                {
                    sb.AppendFormat("\"{0}\"", entity.Fields[i].Name.Value);
                    if (i < entity.Fields.Count - 1)
                        sb.Append(", ");
                }
                sb.Append(" };");
                sb.NewLine();
            }
            foreach (var field in entity.Fields)
            {
                sb.NewLine(tableNum + 1).Append(StructFieldToMemberString(field)).NewLine();
            }
            sb.NewLine();
            
            if (entity.IsMessage)
            {
                var msgEntity = (MessageEnity)entity;
                sb.NewLine(tableNum + 1).Append("public int ID(){ return ").Append(MsgIdToString(msgEntity.ID)).Append("; }");
                sb.NewLine();
            }
            sb.NewLine(tableNum + 1).Append("public void Visit(Sdp.ISdp sdp)");
            sb.NewLine(tableNum + 1).Append('{');
            for (int i=0; i<entity.Fields.Count; ++i)
            {
                var fd = entity.Fields[i];
                if (fd.Type.TypeType == FieldType.Enum)
                {
                    sb.NewLine(tableNum + 2).AppendFormat("sdp.VisitEunm({0}, _member_name_[{1}], false, ref {2});",
                        fd.Index.Value, i, fd.Name.Value);
                } 
                else
                {
                    sb.NewLine(tableNum + 2).AppendFormat("sdp.Visit({0}, _member_name_[{1}], false, ref {2});",
                        fd.Index.Value, i, fd.Name.Value);
                }
            }
            sb.NewLine(tableNum + 1).Append("}");
            sb.NewLine(tableNum).Append("}");
            sb.NewLine();
            return sb.ToString();
        }

        public string RpcToProxyString(RpcEntity entity, int tableNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.NewLine(tableNum).AppendFormat("public void {0}( ", entity.Name.Value);
            if (entity.Param != null)
                sb.AppendFormat("{0} {1}", TypeToString(entity.Param.Type), entity.Param.Name.Value);
            if (entity.ReturnType.TypeType != FieldType.Void)
            {
                if (entity.Param != null)
                    sb.Append(", ");
                sb.AppendFormat("Action<LibActor.ActorID, {0}> handle", TypeToString(entity.ReturnType));
            }
            sb.Append(" )");
            sb.NewLine(tableNum).Append("{");
            if (entity.ReturnType.TypeType != FieldType.Void)
            {
                sb.NewLine(tableNum + 1).AppendFormat("int responid = actor.MsgHandle.AddHandle({0}, RemoteIDs, handle);", MsgIdToString(entity.ID));
            }
            if (entity.Param != null && entity.ReturnType.TypeType != FieldType.Void)
            {
                sb.NewLine(tableNum + 1).AppendFormat("actor.SendMessage({0}, RemoteIDs, {1}, responid);", entity.Param.Name.Value, MsgIdToString(entity.ID));
            }
            if (entity.Param == null && entity.ReturnType.TypeType != FieldType.Void)
            {
                sb.NewLine(tableNum + 1).AppendFormat("actor.SendMessage(RemoteIDs, {0}, responid);", MsgIdToString(entity.ID));
            }
            if (entity.Param != null && entity.ReturnType.TypeType == FieldType.Void)
            {
                sb.NewLine(tableNum + 1).AppendFormat("actor.SendMessage({0}, RemoteIDs, {1});", entity.Param.Name.Value, MsgIdToString(entity.ID));
            }
            if (entity.Param == null && entity.ReturnType.TypeType == FieldType.Void)
            {
                sb.NewLine(tableNum + 1).AppendFormat("actor.SendMessage(RemoteIDs, {1});", MsgIdToString(entity.ID));
            }
            sb.NewLine(tableNum).Append("}");

            return sb.ToString();
        }

        public string RpcToServiceString(RpcEntity entity, int tableNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.NewLine(tableNum).AppendFormat("public virtual {0} {1}(LibActor.ActorID from", TypeToString(entity.ReturnType), entity.Name.Value);
            if (entity.Param != null)
            {
                sb.AppendFormat(", {0} {1}", TypeToString(entity.Param.Type), entity.Param.Name.Value);
            }
            sb.Append(")");
            sb.NewLine(tableNum).Append("{");
            sb.NewLine(tableNum + 1).Append("throw new NotImplementedException();");
            sb.NewLine(tableNum).Append("}");
            return sb.ToString();
        }

        public string RpcToRegistString(RpcEntity entity)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("actor.MsgHandle.AddHandle");
            if (entity.Param != null)
            {
                if (entity.ReturnType.TypeType == FieldType.Void)
                {
                    sb.Append("<").Append(TypeToString(entity.ReturnType)).Append(">");
                } 
                else
                {
                    sb.AppendFormat("<{0}, {1}>", TypeToString(entity.Param.Type), TypeToString(entity.ReturnType));
                }
            }
            sb.AppendFormat("({0}, {1});", MsgIdToString(entity.ID), entity.Name.Value);
            return sb.ToString();
        }

        public string ServiceToString(ServiceEntity entity, string nameSpace, int tableNum)
        {
            StringBuilder sb = new StringBuilder();
            #region Proxy 
            sb.NewLine(tableNum).Append("public class ").Append(entity.Name.Value).Append("Proxy : LibActor.RpcProxy");
            sb.NewLine(tableNum).Append("{");
            //sb.NewLine(tableNum + 1).AppendFormat("public {0}Proxy(LibActor.Actor act){1} actor = act; {2}", entity.Name.Value, "{", "}").NewLine();
            /*
            string fullName = null;
            if (string.IsNullOrEmpty(nameSpace))
            {
                fullName = string.Format("{0}Proxy", entity.Name.Value);
            }
            else
            {
                fullName = string.Format("{0}.{1}Proxy", nameSpace, entity.Name.Value);
            }
            sb.NewLine(tableNum + 1).AppendFormat("public override int GetTypeID() {0} return {1}; {2}", "{", fullName.GetHashCode(), "}").NewLine();
            */
            foreach (var rpc in entity.Rpcs)
            {
                sb.Append(RpcToProxyString(rpc, tableNum + 1)).NewLine();
            }

            sb.NewLine(tableNum).Append("}").NewLine();
            #endregion

            #region service
            sb.NewLine(tableNum).Append("public abstract class ").Append(entity.Name.Value).Append("Service");
            sb.NewLine(tableNum).Append("{").NewLine();

            sb.NewLine(tableNum+1).Append("public void Register(LibActor.Actor actor)");
            sb.NewLine(tableNum + 1).Append("{");
            foreach (var rpc in entity.Rpcs)
            {
                sb.NewLine(tableNum + 2).Append(RpcToRegistString(rpc));
            }
            sb.NewLine(tableNum + 1).Append("}").NewLine();

            foreach (var rpc in entity.Rpcs)
            {
                sb.Append(RpcToServiceString(rpc, tableNum + 1)).NewLine();
            }

            sb.NewLine(tableNum).Append("}").NewLine();
            #endregion

            return sb.ToString();
        }

        private string NameSpaceToString(NameSpaceEntity entity)
        {
            StringBuilder sb = new StringBuilder();
            int tableNum = 0;
            if (!string.IsNullOrEmpty( entity.Name.Value ))
            {
                tableNum = 1;
                sb.AppendFormat("namespace {0}", entity.Name.Value).NewLine().Append("{");
            }

            foreach (var en in entity.Enums)
            {
                sb.Append(EnumToString(en, tableNum));
            }

            foreach (var st in entity.Structs)
            {
                sb.Append(StructToString(st, tableNum));
            }

            foreach (var sv in entity.Services)
            {
                sb.Append(ServiceToString(sv, entity.Name.Value, tableNum));
            }

            if (!string.IsNullOrEmpty(entity.Name.Value))
                sb.Append('}');

            sb.NewLine();
            return sb.ToString();
        }

    }
}
