using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sdp
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UnionIntDouble
    {
        [FieldOffset(0)]
        public ulong Integer;

        [FieldOffset(0)]
        public double Double;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct UnionIntFloat
    {
        [FieldOffset(0)]
        public uint Integer;

        [FieldOffset(0)]
        public float Float;
    }
    public enum SdpPackDataType : byte
    {
        SdpPackDataType_Integer_Positive,
        SdpPackDataType_Integer_Negative,
        SdpPackDataType_Float,
        SdpPackDataType_Double,
        SdpPackDataType_String,
        SdpPackDataType_Vector,
        SdpPackDataType_Map,
        SdpPackDataType_StructBegin,
        SdpPackDataType_StructEnd
    }

    public interface ISdp
    {
        void Visit(uint tag, string name, bool require, ref bool val);
        void Visit(uint tag, string name, bool require, ref int val);
        void Visit(uint tag, string name, bool require, ref uint val);
        void Visit(uint tag, string name, bool require, ref long val);
        void Visit(uint tag, string name, bool require, ref ulong val);
        void Visit(uint tag, string name, bool require, ref float val);
        void Visit(uint tag, string name, bool require, ref double val);
        void Visit(uint tag, string name, bool require, ref string val);
        void Visit(uint tag, string name, bool require, ref DateTime val);
        void Visit(uint tag, string name, bool require, ref Guid val);
        void Visit(uint tag, string name, bool require, ref byte[] val);
        void Visit(uint tag, string name, bool require, ref IMessage val);
        void Visit<T>(uint tag, string name, bool require, ref List<T> val);
        void Visit<TKey, TValue>(uint tag, string name, bool require, ref Dictionary<TKey, TValue> val);
    }

    public interface IMessage
    {
        void Visit(ISdp sdp);
    }

    public interface ISerializer
    {
        object Read(SdpReader reader, uint tag, bool require, object value = null);

        void Write(object value, SdpWriter writer, uint tag, bool require);

    }

    public static class Sdp
    {
        public static DateTime EpochOrigin = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        private static Int32Serializer _Int32 = new Int32Serializer();

        private static UInt32Serializer _UInt32 = new UInt32Serializer();

        private static Int64Serializer _Int64 = new Int64Serializer();

        private static UInt64Serializer _Uint64 = new UInt64Serializer();

        private static BoolSerializer _Bool = new BoolSerializer();

        private static FloatSerializer _Flaot = new FloatSerializer();

        private static DoubleSerializer _Double = new DoubleSerializer();

        private static StringSerializer _String = new StringSerializer();

        private static DateTimeSerializer _DateTime = new DateTimeSerializer();

        private static BytesSerializer _Bytes = new BytesSerializer();

        private static GuidSerializer _Guid = new GuidSerializer();

        private static MessageSerializer _Message = new MessageSerializer();

        private static Type _MessageType = typeof(IMessage);

        private static Dictionary<Type, ISerializer> _SerializerMap = new Dictionary<Type, ISerializer>()
        {
            { typeof(int), _Int32 },
            { typeof(uint), _UInt32 },
            { typeof(long), _Int64 },
            { typeof(ulong), _Uint64 },
            { typeof(bool), _Bool },
            { typeof(float), _Flaot },
            { typeof(double), _Double },
            { typeof(string), _String },
            { typeof(Guid), _Guid },
            { typeof(DateTime),  _DateTime},
            { typeof(byte[]), _Bytes },
        };

        public static ISerializer GetSerializer<T>()
        {
            Type type = typeof(T);
            if (_SerializerMap.ContainsKey(type))
            {
                return _SerializerMap[type];
            }
            foreach(var it in type.GetInterfaces())
            {
                if (it == _MessageType)
                {
                    return _Message;
                }
            }
            throw new Exception("Sdp Wrong type");
        }

        public static T DeepClone<T>( this T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = bf.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }

        public static byte[] Serializer<T>(T val)
        {
            SdpWriter writer = new SdpWriter();
            var ser = GetSerializer<T>();
            if (ser != null)
            {
                ser.Write(val, writer, 0, true);
            }
            return writer.ToBytes();
        }

        public static byte[] Serializer<T>(List<T> val)
        {
            SdpWriter writer = new SdpWriter();
            writer.Visit(0, null, true, ref val);
            return writer.ToBytes();
        }

        public static byte[] Serializer<TKey, TValue>(Dictionary<TKey, TValue> val)
        {
            SdpWriter writer = new SdpWriter();
            writer.Visit(0, null, true, ref val);
            return writer.ToBytes();
        }
        
        public static bool Deserialize<T>(this T val, byte[] data)
        {
            SdpReader reader = new SdpReader(data, 0, 0);
            var ser = GetSerializer<T>();
            if (ser != null)
            {
                val = (T)ser.Read(reader, 0, true, val);
                return true;
            }
            return false;
        }

        public static bool Deserialize<T>(this List<T> val, byte[] data)
        {
            SdpReader reader = new SdpReader(data, 0, 0);
            reader.Visit(0, null, false, ref val);
            return true;
        }

        public static bool Deserialize<TKey, TValue>(this Dictionary<TKey, TValue> val, byte[] data)
        {
            SdpReader reader = new SdpReader(data, 0, 0);
            reader.Visit(0, null, false, ref val);
            return true;
        }
    }
}
