using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sdp
{
    public class SdpWriter : ISdp
    {

        private MemoryStream _Stream;

        private uint _CurPos;

        public SdpWriter()
        {
            _Stream = new MemoryStream();
            _CurPos = 0u;
        }

        public SdpWriter(MemoryStream stream)
        {
            _Stream = stream;
            _CurPos = (uint)stream.Position;
        }

        public bool Pack<T>(T val, uint tag = 0)
        {
            var ser = Sdp.GetSerializer<T>();
            if (ser != null)
            {
                ser.Write(val, this, tag, true);
                return true;
            }
            else
            {
                Type type = typeof(T);
                if (type.IsEnum)
                {
                    VisitEunm(tag, null, true, ref val);
                    return true;
                }
                else
                {
                    foreach (var it in type.GetInterfaces())
                    {
                        if (it == typeof(IDictionary))
                        {
                            Type[] genericTypes = type.GetGenericArguments();
                            IDictionary dir = (IDictionary)val;
                            var keySer = Sdp.GetSerializer(genericTypes[0]);
                            var valSer = Sdp.GetSerializer(genericTypes[1]);
                            Visit(tag, null, true, ref dir, keySer, genericTypes[0], valSer, genericTypes[1]);
                            return true;
                        }
                        else if (it == typeof(IList))
                        {
                            Type[] genericTypes = type.GetGenericArguments();
                            IList list = (IList)val;
                            var serT = Sdp.GetSerializer(genericTypes[0]);
                            Visit(tag, null, true, ref list, serT, genericTypes[0]);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void PackInt32(uint iTag, int iValue)
        {
            if (iValue < 0)
            {
                PackHead(iTag, SdpPackDataType.SdpPackDataType_Integer_Negative);
                PackNum32((uint)(-(uint)iValue));
            }
            else
            {
                PackHead(iTag, SdpPackDataType.SdpPackDataType_Integer_Positive);
                PackNum32((uint)iValue);
            }
        }

        public void PackInt64(uint iTag, long iValue)
        {
            if (iValue < 0L)
            {
                PackHead(iTag, SdpPackDataType.SdpPackDataType_Integer_Negative);
                PackNum64((ulong)(-iValue));
            }
            else
            {
                PackHead(iTag, SdpPackDataType.SdpPackDataType_Integer_Positive);
                PackNum64((ulong)iValue);
            }
        }

        public void PackHead(uint iTag, SdpPackDataType eType)
        {
            byte head = (byte)((int)eType << 4);
            if (iTag < 15u)
            {
                head |= (byte)iTag;
                WriteRawByte(head);
            }
            else
            {
                head |= 15;
                WriteRawByte(head);
                PackNum32(iTag);
            }
        }

        public void PackNum32(uint val)
        {
            while (val > 127u)
            {
                byte raw = (byte)((val & 127u) | 128u);
                WriteRawByte(raw);
                val >>= 7;
            }
            WriteRawByte((byte)val);
        }

        public void PackNum64(ulong val)
        {
            while (val > 127uL)
            {
                byte raw = (byte)((val & 127uL) | 128uL);
                WriteRawByte(raw);
                val >>= 7;
            }
            WriteRawByte((byte)val);
        }

        public uint CurrPos()
        {
            return _CurPos;
        }

        public void WriteRawByte(byte bval)
        {
            _Stream.WriteByte(bval);
            _CurPos += 1u;
        }

        public void WriteRawByte(byte[] val)
        {
            _Stream.Write(val, 0, val.Length);
            _CurPos += (uint)val.Length;
        }

        public byte[] ToBytes()
        {
            return _Stream.ToArray();
        }

        public MemoryStream Stream()
        {
            return _Stream;
        }

        public void Visit(uint tag, string name, bool require, ref bool val)
        {
            if (val || require)
            {
                PackInt32(tag, val ? 1 : 0);
            }
        }

        public void Visit(uint tag, string name, bool require, ref int val)
        {
            if (val != 0 || require)
            {
                PackInt32(tag, val);
            }
        }

        public void Visit(uint tag, string name, bool require, ref uint val)
        {
            if (val !=0 || require)
            {
                PackInt32(tag, (int)val);
            }
        }

        public void Visit(uint tag, string name, bool require, ref long val)
        {
            if (val != 0 || require)
            {
                PackInt64(tag, val);
            }
        }

        public void Visit(uint tag, string name, bool require, ref ulong val)
        {
            if (val != 0 || require)
            {
                PackInt64(tag, (long)val);
            }

        }

        public void Visit(uint tag, string name, bool require, ref float val)
        {
            UnionIntFloat st;
            st.Integer = 0u;
            st.Float = val;
            if (st.Integer != 0 || require)
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_Float);
                PackNum32(st.Integer);
            }
        }

        public void Visit(uint tag, string name, bool require, ref double val)
        {
            UnionIntDouble st;
            st.Integer = 0uL;
            st.Double = val;
            if (st.Integer != 0 || require)
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_Double);
                PackNum64(st.Integer);
            }
        }

        public void Visit(uint tag, string name, bool require, ref string val)
        {
            if (require || !string.IsNullOrEmpty(val))
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_String);
                byte[] vByte = Encoding.Default.GetBytes(val);
                PackNum32((uint)vByte.Length);
                WriteRawByte(vByte);
            }
        }

        public void VisitEunm<T>(uint tag, string name, bool require, ref T val)
        {
            if ((int)(object)val != 0 || require)
            {
                PackInt32(tag, (int)(object)val);
            }
        }

        public void Visit(uint tag, string name, bool require, ref DateTime val)
        {
            if (require || val > Sdp.EpochOrigin)
            {
                ulong iSeconds = (ulong)(val - Sdp.EpochOrigin).TotalSeconds;
                PackHead(tag, SdpPackDataType.SdpPackDataType_Integer_Positive);
                PackNum64(iSeconds);
            }
        }
        
        public void Visit(uint tag, string name, bool require, ref Guid val)
        {
            if (require || val != Guid.Empty)
            {
                string str = val.ToString();
                Visit(tag, name, require, ref str);
            }
        }

        public void Visit(uint tag, string name, bool require, ref byte[] val)
        {
            if (require || (val != null && val.Length > 0))
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_String);
                PackNum32((uint)val.Length);
                WriteRawByte(val);
            }
        }

        public void Visit(uint tag, string name, bool require, ref IStruct val)
        {
            if (val == null)
                return;
            if (require)
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_StructBegin);
                val.Visit(this);
                PackHead(0u, SdpPackDataType.SdpPackDataType_StructEnd);
            }
            else
            {
                SdpWriter sdp = new SdpWriter();
                sdp.PackHead(tag, SdpPackDataType.SdpPackDataType_StructBegin);
                uint iStartPos = sdp.CurrPos();
                val.Visit(sdp);
                if (iStartPos < sdp.CurrPos())
                {
                    sdp.PackHead(0u, SdpPackDataType.SdpPackDataType_StructEnd);
                    WriteRawByte(sdp.ToBytes());
                }
            }
        }

        public void Visit<T>(uint tag, string name, bool require, ref List<T> val)
        {
            if (require || val.Count > 0)
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_Vector);
                PackNum32((uint)val.Count);
                ISerializer ser = Sdp.GetSerializer<T>();
                foreach(var t in val)
                {
                    ser.Write(t, this, 0, true);
                }
            }
        }

        public void Visit<TKey, TValue>(uint tag, string name, bool require, ref Dictionary<TKey, TValue> val)
        {
            if (require || val.Count > 0)
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_Map);
                PackNum32((uint)val.Count);
                ISerializer keySer = Sdp.GetSerializer<TKey>();
                ISerializer valSer = Sdp.GetSerializer<TValue>();
                foreach(var pair in val)
                {
                    keySer.Write(pair.Key, this, 0, true);
                    valSer.Write(pair.Value, this, 0, true);
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref IList val, ISerializer ser, Type typeT)
        {
            if (require || val.Count > 0)
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_Vector);
                PackNum32((uint)val.Count);
                foreach (var t in val)
                {
                    ser.Write(t, this, 0, true);
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref IDictionary val, ISerializer keySer, Type keyType, ISerializer valSer, Type valType)
        {
            if (require || val.Count > 0)
            {
                PackHead(tag, SdpPackDataType.SdpPackDataType_Map);
                PackNum32((uint)val.Count);
                
                foreach (var pair in val)
                {
                    keySer.Write(((DictionaryEntry)pair).Key, this, 0, true);
                    valSer.Write(((DictionaryEntry)pair).Value, this, 0, true);
                }
            }
        }

    }
}
