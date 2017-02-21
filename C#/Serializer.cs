using System;

namespace Sdp
{
    public class BoolSerializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            bool val = false;
            reader.Visit(tag, null, require, ref val);
            return val;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            bool bValue = (bool)value;
            writer.Visit(tag, null, require, ref bValue);
        }
    }

    public class Int32Serializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            int val = 0;
            reader.Visit(tag, null, require, ref val);
            return val;
        }
        
        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            int iValue = (int)value;
            writer.Visit(tag, null, require, ref iValue);
        }
    }

    public class UInt32Serializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            uint val = 0u;
            reader.Visit(tag, null, require, ref val);
            return val;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            uint iValue = (uint)value;
            writer.Visit(tag, null, require, ref iValue);
        }
    }

    public class Int64Serializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            long val = 0L;
            reader.Visit(tag, null, require, ref val);
            return val;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            long iValue = (long)value;
            writer.Visit(tag, null, require, ref iValue);
        }
    }

    public class UInt64Serializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            ulong val = 0uL;
            reader.Visit(tag, null, require, ref val);
            return val;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            ulong iValue = (ulong)value;
            writer.Visit(tag, null, require, ref iValue);
        }
    }

    public class FloatSerializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            float val = 0f;
            reader.Visit(tag, null, require, ref val);
            return val;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            float fValue = (float)value;
            writer.Visit(tag, null, require, ref fValue);
        }
    }

    public class DoubleSerializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            double val = 0.0;
            reader.Visit(tag, null, require, ref val);
            return val;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            double fValue = (double)value;
            writer.Visit(tag, null, require, ref fValue);
        }
    }

    public class DateTimeSerializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            DateTime time = Sdp.EpochOrigin;
            reader.Visit(tag, null, require, ref time);
            return time;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            DateTime time = (DateTime)value;
            writer.Visit(tag, null, require, ref time);
        }
    }

    public class StringSerializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            string val = "";
            reader.Visit(tag, null, require, ref val);
            return val;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            string str = (string)value;
            writer.Visit(tag, null, require, ref str);
        }
    }

    public class BytesSerializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            byte[] val = null;
            reader.Visit(tag, null, require, ref val);
            return val;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            byte[] str = (byte[])value;
            writer.Visit(tag, null, require, ref str);
        }
    }

    public class GuidSerializer : ISerializer
    {
        public static string Empty = Guid.Empty.ToString();
        public object Read(SdpReader reader, uint tag, bool require, object value = null)
        {
            string val = Empty;
            reader.Visit(tag, null, require, ref val);
            return Guid.Parse(val);
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            string str = ((Guid)value).ToString();
            writer.Visit(tag, null, require, ref str);
        }
    }

    public class MessageSerializer : ISerializer
    {
        public object Read(SdpReader reader, uint tag, bool require, object value)
        {
            IMessage t = value as IMessage;
            reader.Visit(tag, null, require, ref t);
            return t;
        }

        public void Write(object value, SdpWriter writer, uint tag, bool require)
        {
            if (value != null)
            {
                IMessage t = (IMessage)value;
                writer.Visit(tag, null, require, ref t);
            }
        }

    }
    
}
