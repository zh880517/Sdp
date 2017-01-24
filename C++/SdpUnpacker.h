#pragma once
#include "SdpHead.h"

class SdpUnpacker
{
public:
	SdpUnpacker() { reset(NULL, 0); }
	explicit SdpUnpacker(const std::string &data) : m_data(NULL), m_size(0), m_pos(0) { reset(data.c_str(), (uint32_t)data.size()); }
	explicit SdpUnpacker(const std::vector<char> &vData) : m_data(NULL), m_size(0), m_pos(0) { reset(&vData[0], (uint32_t)vData.size()); }
	SdpUnpacker(const void *data, uint32_t size) { reset(data, size); }
	void reset() { reset(NULL, 0); }
	void reset(const std::string &data) { reset(data.c_str(), (uint32_t)data.size()); }
	void reset(const void *data, uint32_t size)
	{
		m_data = static_cast<const uint8_t*>(data);
		m_size = size;
		m_pos = 0;
	}

	// redirection
	template <typename T>
	void visit(uint32_t tag, bool require, const char *name, T &val)
	{
		unpack(tag, require, name, val);
	}

	// unpacking interface
	template <typename T>
	void unpack(T &val)
	{
		unpack(0, true, NULL, val);
	}

	void unpack(uint32_t tag, bool require, const char *name, bool &val)
	{
		uint32_t v = val ? 1 : 0;
		unpack(tag, require, name, v);
		val = v ? true : false;
	}
	void unpack(uint32_t tag, bool require, const char *name, char &val)
	{
		uint32_t v = static_cast<uint8_t>(val);
		unpack(tag, require, name, v);
		val = static_cast<char>(v);
	}
	void unpack(uint32_t tag, bool require, const char *name, int8_t &val)
	{
		int32_t v = val;
		unpack(tag, require, name, v);
		val = static_cast<int8_t>(v);
	}
	void unpack(uint32_t tag, bool require, const char *name, uint8_t &val)
	{
		uint32_t v = val;
		unpack(tag, require, name, v);
		val = static_cast<uint8_t>(v);
	}
	void unpack(uint32_t tag, bool require, const char *name, int16_t &val)
	{
		int32_t v = val;
		unpack(tag, require, name, v);
		val = static_cast<int16_t>(v);
	}
	void unpack(uint32_t tag, bool require, const char *name, uint16_t &val)
	{
		uint32_t v = val;
		unpack(tag, require, name, v);
		val = static_cast<uint16_t>(v);
	}
	void unpack(uint32_t tag, bool require, const char *name, int32_t &val)
	{
		uint32_t v = static_cast<uint32_t>(val);
		unpack(tag, require, name, v);
		val = static_cast<int32_t>(v);
	}
	void unpack(uint32_t tag, bool require, const char *name, uint32_t &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_Integer_Negative)
				{
					uint32_t v = val;
					unpackNumber(v);
					val = static_cast<uint32_t>(-v);
				}
				else if (type == SdpPackDataType_Integer_Positive)
				{
					unpackNumber(val);
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}
	void unpack(uint32_t tag, bool require, const char *name, int64_t &val)
	{
		uint64_t v = static_cast<uint64_t>(val);
		unpack(tag, require, name, v);
		val = static_cast<int64_t>(v);
	}
	void unpack(uint32_t tag, bool require, const char *name, uint64_t &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_Integer_Negative)
				{
					uint64_t v = val;
					unpackNumber(v);
					val = static_cast<uint64_t>(-v);
				}
				else if (type == SdpPackDataType_Integer_Positive)
				{
					unpackNumber(val);
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}
	void unpack(uint32_t tag, bool require, const char *name, float &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_Float)
				{
					union { float f; uint32_t v; };
					f = val;
					unpackNumber(v);
					val = f;
				}
				else if (type == SdpPackDataType_Double)
				{
					union { double d; uint64_t v; };
					d = val;
					unpackNumber(v);
					val = static_cast<float>(d);
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}
	void unpack(uint32_t tag, bool require, const char *name, double &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_Float)
				{
					union { float f; uint32_t v; };
					f = static_cast<float>(val);
					unpackNumber(v);
					val = f;
				}
				else if (type == SdpPackDataType_Double)
				{
					union { double d; uint64_t v; };
					d = val;
					unpackNumber(v);
					val = d;
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}
	void unpack(uint32_t tag, bool require, const char *name, std::string &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_String)
				{
					uint32_t size;
					unpackNumber(size);
					val.resize(size);
					unpackData(&val[0], size);
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}
	template <typename Alloc>
	void unpack(uint32_t tag, bool require, const char *name, std::vector<char, Alloc> &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_String)
				{
					uint32_t size;
					unpackNumber(size);
					val.resize(size);
					unpackData(&val[0], size);
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}

	template <typename T, typename Alloc>
	void unpack(uint32_t tag, bool require, const char *name, std::vector<T, Alloc> &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_Vector)
				{
					uint32_t size;
					unpackNumber(size);
					val.resize(size);
					for (uint32_t i = 0; i < size; ++i)
					{
						unpack(val[i]);
					}
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}

	template <typename T>
	void unpack(uint32_t tag, bool require, const char *name, SdpVectorProxy<T> &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_Vector)
				{
					uint32_t size;
					unpackNumber(size);
					for (uint32_t i = 0; i < size; ++i)
					{
						val.under.visit(*this, 0, true, NULL);
					}
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}

	template <typename Key, typename Value, typename Compare, typename Alloc>
	void unpack(uint32_t tag, bool require, const char *name, std::map<Key, Value, Compare, Alloc> &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_Map)
				{
					uint32_t size;
					unpackNumber(size);
					for (uint32_t i = 0; i < size; ++i)
					{
						Key key;
						unpack(key);
						Value &value = val[key];
						unpack(value);
					}
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}

	template <typename T>
	void unpack(uint32_t tag, bool require, const char *name, SdpMapProxy<T> &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_Map)
				{
					uint32_t size;
					unpackNumber(size);
					for (uint32_t i = 0; i < size; ++i)
					{
						val.under.visit(*this, 0, true, NULL);
					}
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}

	template <typename T>
	void unpack(uint32_t tag, bool require, const char *name, T &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_StructBegin)
				{
					val.visit(*this, true); // with all optional field
					skipToStructEnd();
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}

	template <typename T>
	void unpack(uint32_t tag, bool require, const char *name, SdpStructProxy<T> &val)
	{
		_SDPUNPACKER_EXCEPT_TRY_
			if (skipToTag(tag, require))
			{
				SdpPackDataType type;
				unpackHeader(tag, type);
				if (type == SdpPackDataType_StructBegin)
				{
					val.under.visit(*this, true);
					skipToStructEnd();
				}
				else
				{
					throwIncompatiableType(type);
				}
			}
		_SDPUNPACKER_EXCEPT_CATCH_THROW_
	}

	// data operation
	void checksize(uint32_t size)
	{
		if (m_size - m_pos < size)
		{
			throwNoEnoughData();
		}
	}
	void skip(uint32_t size)
	{
		checksize(size);
		m_pos += size;
	}
	void unpackData(void *p, uint32_t size)
	{
		checksize(size);
		memcpy(p, m_data + m_pos, size);
		m_pos += size;
	}
	uint32_t peekHeader(uint32_t &tag, SdpPackDataType &type)
	{
		uint32_t n = 1;
		checksize(1);
		type = static_cast<SdpPackDataType>(m_data[m_pos] >> 4);
		tag = m_data[m_pos] & 0xf;
		if (tag == 0xf)
		{
			m_pos += 1;
			n += peekNumber(tag);
			m_pos -= 1;
		}
		return n;
	}
	void unpackHeader(uint32_t &tag, SdpPackDataType &type)
	{
		checksize(1);
		type = static_cast<SdpPackDataType>(m_data[m_pos] >> 4);
		tag = m_data[m_pos] & 0xf;
		m_pos += 1;
		if (tag == 0xf)
		{
			unpackNumber(tag);
		}
	}
	uint32_t peekNumber(uint32_t &val)
	{
		uint32_t n = 1;
		checksize(1);
		val = m_data[m_pos] & 0x7f;
		while (m_data[m_pos + n - 1] > 0x7f)
		{
			checksize(n);
			uint32_t hi = (m_data[m_pos + n] & 0x7f);
			val |= hi << (7 * n);
			++n;
		}
		return n;
	}
	void unpackNumber(uint32_t &val)
	{
		uint32_t n = peekNumber(val);
		skip(n);
	}
	uint32_t peekNumber(uint64_t &val)
	{
		uint32_t n = 1;
		checksize(1);
		val = m_data[m_pos] & 0x7f;
		while (m_data[m_pos + n - 1] > 0x7f)
		{
			checksize(n);
			uint64_t hi = (m_data[m_pos + n] & 0x7f);
			val |= hi << (7 * n);
			++n;
		}
		return n;
	}
	void unpackNumber(uint64_t &val)
	{
		uint32_t n = peekNumber(val);
		skip(n);
	}
	bool skipToTag(uint32_t tag, bool require)
	{
		uint32_t curtag;
		SdpPackDataType curtype;
		while (m_pos < m_size)
		{
			uint32_t n = peekHeader(curtag, curtype);
			if (curtype == SdpPackDataType_StructEnd || curtag > tag)
			{
				break;
			}
			if (curtag == tag)
			{
				return true;
			}
			skip(n);
			skipField(curtype);
		}
		if (require)
		{
			throwFieldNotExist();
		}
		return false;
	}
	void skipToStructEnd()
	{
		uint32_t curtag;
		SdpPackDataType curtype;
		while (true)
		{
			unpackHeader(curtag, curtype);
			if (curtype == SdpPackDataType_StructEnd)
			{
				break;
			}
			skipField(curtype);
		}
	}
	void skipField(SdpPackDataType type)
	{
		switch (type)
		{
		case SdpPackDataType_Integer_Positive:
		case SdpPackDataType_Integer_Negative:
		case SdpPackDataType_Float:
		case SdpPackDataType_Double:
		{
			uint64_t val;
			unpackNumber(val);
		}
		break;
		case SdpPackDataType_String:
		{
			uint32_t size;
			unpackNumber(size);
			skip(size);
		}
		break;
		case SdpPackDataType_Vector:
		{
			uint32_t size;
			unpackNumber(size);
			for (uint32_t i = 0; i < size; ++i)
			{
				skipField();
			}
		}
		break;
		case SdpPackDataType_Map:
		{
			uint32_t size;
			unpackNumber(size);
			for (uint32_t i = 0; i < size; ++i)
			{
				skipField();
				skipField();
			}
		}
		break;
		case SdpPackDataType_StructBegin:
			skipToStructEnd();
			break;
		case SdpPackDataType_StructEnd:
			break;
		default:
			throwUnknowDataType(type);
			break;
		}
	}
	void skipField()
	{
		uint32_t curtag;
		SdpPackDataType curtype;
		unpackHeader(curtag, curtype);
		skipField(curtype);
	}
	void throwIncompatiableType(SdpPackDataType type)
	{
		char buf[322];
		snprintf(buf, sizeof(buf), "got wrong type %d", type);
		throw SdpException(buf);
	}
	void throwFieldNotExist()
	{
		throw SdpException("field not exist");
	}
	void throwNoEnoughData()
	{
		throw SdpException("end of data");
	}
	void throwUnknowDataType(SdpPackDataType type)
	{
		char buf[322];
		snprintf(buf, sizeof(buf), "unknown type %d", type);
		throw SdpException(buf);
	}

private:
	const uint8_t 	*m_data;
	uint32_t		m_size;
	uint32_t		m_pos;
};
