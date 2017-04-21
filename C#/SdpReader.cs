using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sdp
{
    public class SdpReader : ISdp
    {
        private byte[] _Stream;

        private uint _CurPos;

        private uint _Size;

        public SdpReader(byte[] stream, uint iCurPos, uint iSize = 0u)
        {
            _Stream = stream;
            _CurPos = iCurPos;
            if (iSize == 0)
            {
                _Size = (uint)_Stream.Length;
            }
            else
            {
                _Size = iSize + iCurPos;
            }
        }

        public bool UnPack<T>(ref T val, uint tag)
        {
            var ser = Sdp.GetSerializer<T>();
            if (val == null)
                val = Activator.CreateInstance<T>();
            if (ser != null)
            {
                val = (T)ser.Read(this, tag, true, val);
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
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        public T UnPack<T>(uint tag)
        {
            T val = Activator.CreateInstance<T>();
            if (UnPack(ref val, tag))
                return val;
            return default(T);
        }

        public bool SkipToTag(uint iTag)
        {
            uint iCurTag = 0u;
            SdpPackDataType eType = SdpPackDataType.SdpPackDataType_Integer_Positive;
            while (_CurPos < _Size)
            {
                uint i = PeekHead(ref iCurTag, ref eType);

                if (eType == SdpPackDataType.SdpPackDataType_StructEnd || iCurTag > iTag)
                    break;

                if (iCurTag == iTag)
                    return true;
                Skip(i);
                SkipField(eType);
            }
            return false;
        }

        public SdpPackDataType UnPackHead(ref uint iTag)
        {
            Checksize(1u);
            uint iType = _Stream[(int)_CurPos];
            iTag = (iType & 15u);
            _CurPos += 1u;
            if (iTag == 15u)
                iTag = Unpack32();

            return (SdpPackDataType)(iType >> 4);
        }

        private uint PeekNumber32(ref uint val)
        {
            uint i = 1u;
            Checksize(1u);
            val = (uint)(_Stream[(int)_CurPos] & 127);
            while (_Stream[(int)(_CurPos + i - 1u)] > 127)
            {
                Checksize(i);
                uint hi = (uint)(_Stream[(int)(_CurPos + i)] & 127);
                val |= hi << (int)(7u * i);
                i += 1u;
            }
            return i;
        }

        private uint PeekNumber64(ref ulong val)
        {
            uint i = 1u;
            Checksize(1u);
            val = (ulong)(_Stream[(int)_CurPos] & 127);
            while (_Stream[(int)(_CurPos + i - 1u)] > 127)
            {
                Checksize(i);
                ulong hi = (ulong)(_Stream[(int)(_CurPos + i)] & 127);
                val |= hi << (int)(7u * i);
                i += 1u;
            }
            return i;
        }

        private void Checksize(uint size)
        {
            if (_Size - _CurPos < size)
            {
                throw new Exception("SdpReader.Checksize() faile!!!");
            }
        }

        private void Skip(uint iSize)
        {
            Checksize(iSize);
            _CurPos += iSize;
        }

        private void SkipField(SdpPackDataType eType)
        {
            switch (eType)
            {
                case SdpPackDataType.SdpPackDataType_Integer_Positive:
                case SdpPackDataType.SdpPackDataType_Integer_Negative:
                case SdpPackDataType.SdpPackDataType_Float:
                case SdpPackDataType.SdpPackDataType_Double:
                    {
                        Unpack64();
                        break;
                    }
                case SdpPackDataType.SdpPackDataType_String:
                    {
                        uint iSize = Unpack32();
                        Skip(iSize);
                        break;
                    }
                case SdpPackDataType.SdpPackDataType_Vector:
                    {
                        uint iSize2 = Unpack32();
                        for (uint i = 0u; i < iSize2; i += 1u)
                        {
                            SkipField();
                        }
                        break;
                    }
                case SdpPackDataType.SdpPackDataType_Map:
                    {
                        uint iSize3 = Unpack32();
                        for (uint j = 0u; j < iSize3; j += 1u)
                        {
                            SkipField();
                            SkipField();
                        }
                        break;
                    }
                case SdpPackDataType.SdpPackDataType_StructBegin:
                    SkipToStructEnd();
                    break;
            }
        }

        private void SkipField()
        {
            uint iTag = 0u;
            SdpPackDataType curtype = UnPackHead(ref iTag);
            SkipField(curtype);
        }

        public void SkipToStructEnd()
        {
            uint curtag = 0u;
            while (true)
            {
                SdpPackDataType curtype = UnPackHead(ref curtag);
                if (curtype == SdpPackDataType.SdpPackDataType_StructEnd)
                    break;

                SkipField(curtype);
            }
        }

        public uint Unpack32()
        {
            uint iValue = 0u;
            uint i = PeekNumber32(ref iValue);
            Skip(i);
            return iValue;
        }

        public ulong Unpack64()
        {
            ulong iValue = 0uL;
            uint i = PeekNumber64(ref iValue);
            Skip(i);
            return iValue;
        }

        public string UnPackString(uint len)
        {
            string str = Encoding.Default.GetString(_Stream, (int)_CurPos, (int)len);
            Skip(len);
            return str;
        }

        public byte[] UnPackBytes(uint len)
        {
            byte[] bytes = new byte[len];
            Array.Copy(_Stream, (int)_CurPos, bytes, 0, (int)len);
            return bytes;
        }

        private uint PeekHead(ref uint iTag, ref SdpPackDataType eType)
        {
            uint i = 1u;
            Checksize(1u);
            eType = (SdpPackDataType)(_Stream[(int)_CurPos] >> 4);
            iTag = (uint)(_Stream[(int)_CurPos] & 15);
            if (iTag == 15u)
            {
                _CurPos += 1u;
                i += PeekNumber32(ref iTag);
                _CurPos -= 1u;
            }
            return i;
        }

        public void Visit(uint tag, string name, bool require, ref bool val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Integer_Positive)
                {
                    uint iValue = Unpack32();
                    val = (iValue == 1u);
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref int val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Integer_Negative)
                {
                    uint iValue = Unpack32();
                    val = -(int)iValue;
                }
                else
                {
                    if (type == SdpPackDataType.SdpPackDataType_Integer_Positive)
                    {
                        uint iValue2 = Unpack32();
                        val = (int)iValue2;
                    }
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref uint val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Integer_Negative)
                {
                    uint iValue = Unpack32();
                    val = (uint)(-iValue);
                }
                else
                {
                    if (type == SdpPackDataType.SdpPackDataType_Integer_Positive)
                    {
                        val = Unpack32();
                    }
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref long val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Integer_Negative)
                {
                    ulong iValue = Unpack64();
                    val = -(long)iValue;
                }
                else
                {
                    if (type == SdpPackDataType.SdpPackDataType_Integer_Positive)
                    {
                        val = (long)Unpack64();
                    }
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref ulong val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Integer_Negative)
                {
                    ulong iValue = Unpack64();
                    val = (ulong)-(long)iValue;
                }
                else
                {
                    if (type == SdpPackDataType.SdpPackDataType_Integer_Positive)
                    {
                        val = Unpack64();
                    }
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref float val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Float)
                {
                    uint iValue = Unpack32();
                    UnionIntFloat st;
                    st.Float = 0f;
                    st.Integer = iValue;
                    val = st.Float;
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref double val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Double)
                {
                    ulong iValue = Unpack64();
                    UnionIntDouble st;
                    st.Double = 0.0;
                    st.Integer = iValue;
                    val = st.Double;
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref string val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_String)
                {
                    uint iLen = Unpack32();
                    if (iLen > 0u)
                    {
                        val = UnPackString(iLen);
                    }
                }
            }
        }

        public void VisitEunm<T>(uint tag, string name, bool require, ref T val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Integer_Negative)
                {
                    uint iValue = Unpack32();
                    val = (T)(object) - (int)iValue;
                }
                else
                {
                    if (type == SdpPackDataType.SdpPackDataType_Integer_Positive)
                    {
                        uint iValue2 = Unpack32();
                        val = (T)(object)(int)iValue2;
                    }
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref Guid val)
        {
            string str = GuidSerializer.Empty;
            Visit(tag, name, require, ref val);
            val = new Guid(str);
        }

        public void Visit(uint tag, string name, bool require, ref DateTime val)
        {
            if (SkipToTag(tag))
            {
                long iSecond = 0L;
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Integer_Positive)
                {
                    iSecond = (long)Unpack64();
                }
                val = Sdp.EpochOrigin.AddSeconds(iSecond);
            }
        }

        public void Visit(uint tag, string name, bool require, ref byte[] val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_String)
                {
                    uint iLen = Unpack32();
                    if (iLen > 0u)
                    {
                        val = UnPackBytes(iLen);
                    }
                }
            }
        }

        public void Visit(uint tag, string name, bool require, ref IStruct val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_StructBegin)
                {
                    val.Visit(this);
                    SkipToStructEnd();
                }
            }
        }

        public void Visit<T>(uint tag, string name, bool require, ref List<T> val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Vector)
                {
                    uint iSize = Unpack32();
                    if (val == null)
                        val = new List<T>((int)iSize);
                    ISerializer ser = Sdp.GetSerializer<T>();
                    for (uint i=0; i<iSize; ++i)
                    {
                        T t = Activator.CreateInstance<T>();
                        t = (T)ser.Read(this, 0, require, t);
                        val.Add(t);
                    }//for
                }//if
            }//if
        }

        public void Visit(uint tag, string name, bool require, ref IList val, ISerializer ser, Type typeT)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Vector)
                {
                    uint iSize = Unpack32();
                    for (uint i = 0; i < iSize; ++i)
                    {
                        object t = Activator.CreateInstance(typeT);
                        t = ser.Read(this, 0, require, t);
                        val.Add(t);
                    }//for
                }//if
            }//if
        }

        public void Visit<TKey, TValue>(uint tag, string name, bool require, ref Dictionary<TKey, TValue> val)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Map)
                {
                    uint iSize = Unpack32();
                    if (val == null)
                        val = new Dictionary<TKey, TValue>((int)iSize);
                    ISerializer keySer = Sdp.GetSerializer<TKey>();
                    ISerializer valSer = Sdp.GetSerializer<TValue>();
                    for (var i=0; i<iSize; ++i)
                    {
                        TKey key = Activator.CreateInstance<TKey>();
                        TValue value = Activator.CreateInstance<TValue>();
                        key = (TKey)keySer.Read(this, 0, require, key);
                        value = (TValue)valSer.Read(this, 0, require, value);
                        if (val.ContainsKey(key))
                            val.Remove(key);
                        val.Add(key, value);
                    }//for
                }//if
            }//if
        }

        public void Visit(uint tag, string name, bool require, ref IDictionary val, ISerializer keySer, Type keyType, ISerializer valSer, Type valType)
        {
            if (SkipToTag(tag))
            {
                SdpPackDataType type = UnPackHead(ref tag);
                if (type == SdpPackDataType.SdpPackDataType_Map)
                {
                    uint iSize = Unpack32();
                    for (var i = 0; i < iSize; ++i)
                    {
                        object key = Activator.CreateInstance(keyType);
                        object value = Activator.CreateInstance(valType);
                        key = keySer.Read(this, 0, require, key);
                        value = valSer.Read(this, 0, require, value);
                        if (val.Contains(key))
                            val.Remove(key);
                        val.Add(key, value);
                    }//for
                }//if
            }//if
        }

    }
}
